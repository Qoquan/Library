// =============================================================
// Fichier : Library.Web/Program.cs
// Rôle    : Point d'entrée du projet Blazor Server.
//           Configure les services, le HttpClient vers l'API,
//           et les pages Razor/Blazor.
// =============================================================

using Library.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 1. Services Blazor Server
// -------------------------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// -------------------------------------------------------
// 2. HttpClient pointant vers l'API interne Library.API
// -------------------------------------------------------
// L'URL de l'API est configurée dans appsettings.json
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

builder.Services.AddHttpClient<BookApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// -------------------------------------------------------
// 3. Construction de l'application
// -------------------------------------------------------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Console.WriteLine($"=== Library Web démarrée ===");
Console.WriteLine($"Connexion API : {apiBaseUrl}");

app.Run();