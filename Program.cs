using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register PdfTranslationService with a configuration for the Google API key
builder.Services.AddSingleton<PdfTranslationService>(provider =>
    new PdfTranslationService("sk-2548c66dd69242e38289ac882310ceea"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=PdfTranslation}/{action=Index}/{id?}");

app.Run();