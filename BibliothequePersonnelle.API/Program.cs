using BibliothequePersonnelle.Core.Interfaces;
using BibliothequePersonnelle.Infrastructure.Data;
using BibliothequePersonnelle.Infrastructure.Repositories;
using BibliothequePersonnelle.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration de la base de données
builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    options.UseSqlite("Data Source=bibliotheque.db"));

// Injection de dépendances
builder.Services.AddScoped<ILivreRepository, LivreRepository>();
builder.Services.AddHttpClient<IExternalBookService, GoogleBooksService>();

// Configuration CORS pour Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Créer la base de données au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BibliothequeDbContext>();
    db.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Classe partielle pour les tests
public partial class Program { }