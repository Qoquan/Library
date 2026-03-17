// =============================================================
// Fichier : Library.Web/Program.cs
// Rôle    : Point d'entrée du projet Blazor Server.
//           Enregistre AuthService comme Scoped (une instance
//           par connexion utilisateur).
// =============================================================

using Library.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

// AuthService : Scoped = une instance par circuit Blazor (par onglet)
builder.Services.AddScoped<AuthService>();

// BookApiService dépend de AuthService → aussi Scoped
builder.Services.AddScoped<BookApiService>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>()
                 .CreateClient("LibraryAPI");
    var auth = sp.GetRequiredService<AuthService>();
    return new BookApiService(http, auth);
});

builder.Services.AddHttpClient("LibraryAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

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

Console.WriteLine("=== Library Web démarrée ===");
Console.WriteLine($"Connexion API : {apiBaseUrl}");

app.Run();