// =============================================================
// Fichier : Library.API/Program.cs
// Rôle    : Point d'entrée de l'API REST (.NET 9)
// =============================================================

using Microsoft.EntityFrameworkCore;
using Library.API.Data;
using Library.API.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 1. Enregistrement des services
// -------------------------------------------------------

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Library API",
        Version = "v1",
        Description = "API REST interne pour la gestion de la bibliothèque."
    });
});

// Base de données SQLite via Entity Framework Core 9
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=library.db"
    )
);

// Services métier (injection de dépendances)
builder.Services.AddScoped<IBookService, BookService>();

// HttpClient pour l'API externe OpenLibrary
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();

// CORS — autorise Blazor à appeler l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7001",
                "http://localhost:5001",
                "http://localhost:5175")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// -------------------------------------------------------
// 2. Construction
// -------------------------------------------------------
var app = builder.Build();

// Migration auto de la base de données
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();
}

// -------------------------------------------------------
// 3. Middlewares
// -------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=== Library API démarrée ===");
Console.WriteLine("Swagger : http://localhost:5000/swagger");
Console.WriteLine("API     : http://localhost:5000/api/books");

app.Run();

// ✅ REQUIS en .NET 9 pour WebApplicationFactory (tests d'intégration)
public partial class Program { }