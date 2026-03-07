// =============================================================
// Fichier : Library.Tests/Integration/LibraryWebFactory.cs
// Rôle    : Fabrique de serveur de test pour les tests d'intégration.
//           Remplace SQLite par InMemory pour les tests.
// =============================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Library.API.Data;
using Library.Tests.Helpers;

namespace Library.Tests.Integration
{
    public class LibraryWebFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"IntegrationTest_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // -------------------------------------------------------
                // ÉTAPE 1 — Supprimer ABSOLUMENT TOUT ce qui touche à EF Core
                // On filtre par FullName pour attraper les services internes
                // -------------------------------------------------------
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<LibraryDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(LibraryDbContext) ||
                        (d.ServiceType.FullName != null && (
                            d.ServiceType.FullName.Contains("EntityFrameworkCore") ||
                            d.ServiceType.FullName.Contains("Sqlite") ||
                            d.ServiceType.FullName.Contains("DbContext")
                        )) ||
                        (d.ImplementationType != null && d.ImplementationType.FullName != null && (
                            d.ImplementationType.FullName.Contains("EntityFrameworkCore") ||
                            d.ImplementationType.FullName.Contains("Sqlite") ||
                            d.ImplementationType.FullName.Contains("DbContext")
                        ))
                    )
                    .ToList();

                foreach (var descriptor in toRemove)
                    services.Remove(descriptor);

                // -------------------------------------------------------
                // ÉTAPE 2 — Enregistrer EF Core avec InMemory uniquement
                // -------------------------------------------------------
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // -------------------------------------------------------
                // ÉTAPE 3 — Seeder la base InMemory avec des données de test
                // -------------------------------------------------------
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                db.Database.EnsureCreated();

                if (!db.Books.Any())
                {
                    foreach (var book in TestDbHelper.GetSampleBooks())
                    {
                        db.Books.Add(new Library.Shared.Models.Book
                        {
                            Title        = book.Title,
                            Author       = book.Author,
                            ISBN         = book.ISBN,
                            PublishedYear = book.PublishedYear,
                            Genre        = book.Genre,
                            IsAvailable  = book.IsAvailable,
                            Source       = book.Source,
                            CreatedAt    = book.CreatedAt
                        });
                    }
                    db.SaveChanges();
                }
            });

            builder.UseEnvironment("Testing");
        }
    }
}