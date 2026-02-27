// =============================================================
// Fichier : Library.Tests/Helpers/TestDbHelper.cs
// Rôle    : Fabrique de contextes de base de données en mémoire.
//           Partagé entre les tests unitaires et d'intégration.
//           Chaque appel crée une BD isolée (GUID unique).
// =============================================================

using Microsoft.EntityFrameworkCore;
using Library.API.Data;
using Library.Shared.Models;

namespace Library.Tests.Helpers
{
    /// <summary>
    /// Classe utilitaire pour créer des contextes EF Core InMemory.
    /// Garantit l'isolation entre les tests : chaque test a sa propre BD.
    /// </summary>
    public static class TestDbHelper
    {
        // -------------------------------------------------------
        // Création d'un contexte InMemory vide
        // -------------------------------------------------------

        /// <summary>
        /// Crée un nouveau LibraryDbContext utilisant une base InMemory.
        /// Le nom GUID garantit qu'aucun test ne partage de données.
        /// </summary>
        public static LibraryDbContext CreateInMemoryContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new LibraryDbContext(options);
        }

        // -------------------------------------------------------
        // Création d'un contexte avec données de test pré-remplies
        // -------------------------------------------------------

        /// <summary>
        /// Crée un contexte InMemory pré-rempli avec des livres de test.
        /// Utile pour les tests qui nécessitent des données existantes.
        /// </summary>
        public static LibraryDbContext CreateSeededContext()
        {
            var context = CreateInMemoryContext();

            var books = GetSampleBooks();
            context.Books.AddRange(books);
            context.SaveChanges();

            return context;
        }

        // -------------------------------------------------------
        // Données de test réutilisables
        // -------------------------------------------------------

        /// <summary>
        /// Retourne une liste de livres de test cohérents.
        /// </summary>
        public static List<Book> GetSampleBooks() => new()
        {
            new Book
            {
                Id = 1,
                Title = "Le Petit Prince",
                Author = "Antoine de Saint-Exupéry",
                ISBN = "978-2-07-040850-4",
                PublishedYear = 1943,
                Genre = "Conte",
                IsAvailable = true,
                Source = "local",
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new Book
            {
                Id = 2,
                Title = "1984",
                Author = "George Orwell",
                ISBN = "978-2-07-036822-8",
                PublishedYear = 1949,
                Genre = "Dystopie",
                IsAvailable = true,
                Source = "local",
                CreatedAt = new DateTime(2024, 1, 2)
            },
            new Book
            {
                Id = 3,
                Title = "Dune",
                Author = "Frank Herbert",
                ISBN = "978-2-07-040850-5",
                PublishedYear = 1965,
                Genre = "Science-Fiction",
                IsAvailable = false,
                Source = "local",
                CreatedAt = new DateTime(2024, 1, 3)
            },
            new Book
            {
                Id = 4,
                Title = "Harry Potter à l'école des sorciers",
                Author = "J.K. Rowling",
                PublishedYear = 1997,
                Genre = "Fantasy",
                IsAvailable = true,
                Source = "openlibrary",
                CreatedAt = new DateTime(2024, 1, 4)
            }
        };

        /// <summary>
        /// Crée un livre valide pour les tests de création.
        /// </summary>
        public static Book CreateValidBook(string title = "Test Book", string author = "Test Author")
        {
            return new Book
            {
                Title = title,
                Author = author,
                Genre = "Fiction",
                PublishedYear = 2024,
                IsAvailable = true,
                Source = "local"
            };
        }
    }
}