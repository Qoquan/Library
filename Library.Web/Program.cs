using Library.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

// Client HTTP nommé pointant vers l'API
builder.Services.AddHttpClient("LibraryAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// AuthService reçoit un HttpClient dédié
builder.Services.AddScoped<AuthService>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LibraryAPI");
    return new AuthService(http);
});

// BookApiService dépend de AuthService
builder.Services.AddScoped<BookApiService>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LibraryAPI");
    var auth = sp.GetRequiredService<AuthService>();
    return new BookApiService(http, auth);
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