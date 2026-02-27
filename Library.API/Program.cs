// =============================================================
// Fichier : Library.API/Program.cs — VERSION .NET 9
// Rôle    : Point d'entrée de l'API REST.
//
// Changements vs .NET 8 :
//   ❌ Swashbuckle.AspNetCore       → retiré (non maintenu)
//   ✅ Microsoft.AspNetCore.OpenApi → OpenAPI natif .NET 9
//   ✅ Scalar.AspNetCore            → UI moderne remplaçant Swagger UI
//   ✅ public partial class Program → requis pour WebApplicationFactory (tests)
// =============================================================

using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Library.API.Data;
using Library.API.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// 1. Enregistrement des services
// -------------------------------------------------------

builder.Services.AddControllers();

// ✅ .NET 9 : OpenAPI natif (remplace AddSwaggerGen + AddEndpointsApiExplorer)
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
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
    // ✅ .NET 9 : expose /openapi/v1.json
    app.MapOpenApi();

    // ✅ .NET 9 : Scalar remplace Swagger UI
    // Accessible sur : http://localhost:5000/scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.Title = "Library API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("=== Library API (.NET 9) ===");
Console.WriteLine("Scalar UI  : http://localhost:5000/scalar/v1");
Console.WriteLine("OpenAPI    : http://localhost:5000/openapi/v1.json");
Console.WriteLine("API Books  : http://localhost:5000/api/books");

app.Run();

// ✅ REQUIS en .NET 9 pour WebApplicationFactory (tests d'intégration)
public partial class Program { }