// =============================================================
// Fichier : Library.Tests/Integration/LibraryWebFactory.cs
// Rôle    : Fabrique de serveur de test pour les tests d'intégration.
//           Lance une vraie instance de Library.API en mémoire,
//           remplace SQLite par InMemory, désactive OpenLibrary réel.
//
// La WebApplicationFactory d'ASP.NET Core démarre l'application
// complète (middlewares, DI, routes) dans le processus de test.
// =============================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Library.API.Data;
using Library.Tests.Helpers;

namespace Library.Tests.Integration
{
    /// <summary>
    /// Fabrique qui configure Library.API pour les tests d'intégration.
    /// Elle remplace :
    ///   - SQLite → EF Core InMemory (rapide, sans fichier)
    ///   - Les données sont seedées via TestDbHelper
    ///
    /// Utilisation :
    ///   var factory = new LibraryWebFactory();
    ///   var client = factory.CreateClient();
    ///   var response = await client.GetAsync("/api/books");
    /// </summary>
    public class LibraryWebFactory : WebApplicationFactory<Program>
    {
        // Nom unique de la BD InMemory pour cette instance de factory
        private readonly string _dbName = $"IntegrationTest_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // -------------------------------------------------------
                // 1. Supprimer la configuration SQLite existante
                // -------------------------------------------------------
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // -------------------------------------------------------
                // 2. Remplacer par EF Core InMemory
                // -------------------------------------------------------
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));

                // -------------------------------------------------------
                // 3. Seeder des données de test dans la BD
                // -------------------------------------------------------
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

                db.Database.EnsureCreated();

                // Insérer des livres de démonstration pour les tests
                if (!db.Books.Any())
                {
                    var sampleBooks = TestDbHelper.GetSampleBooks();
                    // Reset les IDs pour que EF les génère proprement
                    foreach (var book in sampleBooks)
                    {
                        db.Books.Add(new Library.Shared.Models.Book
                        {
                            Title = book.Title,
                            Author = book.Author,
                            ISBN = book.ISBN,
                            PublishedYear = book.PublishedYear,
                            Genre = book.Genre,
                            IsAvailable = book.IsAvailable,
                            Source = book.Source,
                            CreatedAt = book.CreatedAt
                        });
                    }
                    db.SaveChanges();
                }
            });

            // Utiliser l'environnement "Testing"
            builder.UseEnvironment("Testing");
        }
    }
}