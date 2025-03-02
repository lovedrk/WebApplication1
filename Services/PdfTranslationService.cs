using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Writer;
using DeepSeek.ApiClient;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer.Fonts;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using Microsoft.Extensions.DependencyInjection;
using DeepSeek.ApiClient.Extensions;
using DeepSeek.ApiClient.Interfaces;
using DeepSeek.ApiClient.Models;
using Newtonsoft.Json;

namespace WebApplication1.Services
{
    public class PdfTranslationService
    {
        private readonly IDeepSeekClient _deepSeekClient;

        public PdfTranslationService(string deepSeekApiKey)
        {
            var services = new ServiceCollection();
            services.AddDeepSeekClient(deepSeekApiKey);
            var serviceProvider = services.BuildServiceProvider();
            _deepSeekClient = serviceProvider.GetRequiredService<IDeepSeekClient>();
        }

        public async Task TranslatePdfAsync(string inputFilePath, string outputFilePath, string targetLanguage)
        {
            using (var document = PdfDocument.Open(inputFilePath))
            using (var pdfWriter = new PdfDocumentBuilder())
            {
                var font = pdfWriter.AddStandard14Font(Standard14Font.Helvetica);

                foreach (var page in document.GetPages())
                {
                    var pageBuilder = pdfWriter.AddPage(page.Width, page.Height);

                    foreach (var word in page.GetWords())
                    {
                        var request = new DeepSeekRequestBuilder()
                            .SetModel(DeepSeekModel.V3)
                            .SetStream(false)
                            .SetTemperature(0)
                            .SetSystemMessage("Translate the following text")
                            .AddUserMessage(word.Text);

                        var responseString = await _deepSeekClient.SendMessageAsync(request.Build());
                        var response = JsonConvert.DeserializeObject<DeepSeekResponse>(responseString);
                        var translatedText = response?.Choices?.FirstOrDefault()?.Message?.Content ?? word.Text;

                        var fontSize = word.Letters.FirstOrDefault()?.FontSize ?? 12; // Default to 12 if font size is not available
                        pageBuilder.AddText(translatedText, fontSize, new PdfPoint(word.BoundingBox.BottomLeft.X, word.BoundingBox.BottomLeft.Y), font);
                    }
                }

                using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                {
                    var pdfBytes = pdfWriter.Build();
                    outputStream.Write(pdfBytes, 0, pdfBytes.Length);
                }
            }
        }

    }
}