using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services;
using System.Threading.Tasks; // Add this using directive

namespace WebApplication1.Controllers
{
    public class PdfTranslationController : Controller
    {
        private readonly PdfTranslationService _pdfTranslationService;

        public PdfTranslationController(PdfTranslationService pdfTranslationService)
        {
            _pdfTranslationService = pdfTranslationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Translate(IFormFile pdfFile, string targetLanguage) // Make the method async
        {
            if (pdfFile != null && !string.IsNullOrEmpty(targetLanguage))
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
                }

                var inputFilePath = Path.Combine(uploadsFolder, pdfFile.FileName);
                var outputFilePath = Path.Combine(uploadsFolder, "translated_" + pdfFile.FileName);

                // Ensure the file is not being used by another process
                if (System.IO.File.Exists(inputFilePath))
                {
                    System.IO.File.Delete(inputFilePath);
                }

                using (var stream = new FileStream(inputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await pdfFile.CopyToAsync(stream); // Use async method
                }

                await _pdfTranslationService.TranslatePdfAsync(inputFilePath, outputFilePath, targetLanguage); // Use async method
                ViewBag.TranslatedFilePath = outputFilePath;
            }

            return View("Index");
        }
    }
}