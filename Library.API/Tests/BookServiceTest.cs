// =============================================================
// Fichier : Library.API/Tests/BookServiceTests.cs
// Rôle    : Tests unitaires du service BookService.
//           AA2 - Concevoir et mettre en oeuvre une procédure de test.
//           Utilise xUnit + EF Core InMemory pour simuler la BD.
//
// POUR UTILISER CES TESTS :
//   1. Créer un projet test séparé : dotnet new xunit -n Library.Tests
//   2. Ajouter les packages :
//      dotnet add package Microsoft.EntityFrameworkCore.InMemory
//      dotnet add package xunit
//   3. Copier ce fichier dans Library.Tests/
//   4. Lancer : dotnet test
// =============================================================

using Microsoft.EntityFrameworkCore;
using Library.API.Data;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Tests
{
    /// <summary>
    /// Tests unitaires pour BookService.
    /// Chaque test est isolé : une base InMemory est créée par test.
    /// POO : principe de test de chaque méthode individuellement.
    /// </summary>
    public class BookServiceTests : IDisposable
    {
        // -------------------------------------------------------
        // Setup : contexte de base de données en mémoire
        // -------------------------------------------------------
        private readonly LibraryDbContext _context;
        private readonly BookService _service;

        public BookServiceTests()
        {
            // On utilise une base InMemory unique par test (GUID)
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryDbContext(options);
            _service = new BookService(_context);
        }

        // -------------------------------------------------------
        // Scénario 1 : GetAllAsync retourne une liste vide au départ
        // -------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoBooksExist()
        {
            // Arrange : base vide

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // -------------------------------------------------------
        // Scénario 2 : CreateAsync crée un livre correctement
        // -------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldAddBook_AndReturnWithId()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Fiction"
            };

            // Act
            var created = await _service.CreateAsync(newBook);

            // Assert
            Assert.NotNull(created);
            Assert.True(created.Id > 0); // L'ID doit avoir été assigné
            Assert.Equal("Test Book", created.Title);
            Assert.Equal("Test Author", created.Author);
            Assert.Equal("local", created.Source);
        }

        // -------------------------------------------------------
        // Scénario 3 : GetByIdAsync retourne null pour un ID inexistant
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Arrange : aucun livre en base

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // -------------------------------------------------------
        // Scénario 4 : GetByIdAsync retourne le bon livre
        // -------------------------------------------------------
        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectBook()
        {
            // Arrange
            var book = await _service.CreateAsync(new Book
            {
                Title = "Dune",
                Author = "Frank Herbert"
            });

            // Act
            var found = await _service.GetByIdAsync(book.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Dune", found.Title);
            Assert.Equal("Frank Herbert", found.Author);
        }

        // -------------------------------------------------------
        // Scénario 5 : UpdateAsync met à jour les données
        // -------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldModifyBook_WhenBookExists()
        {
            // Arrange
            var book = await _service.CreateAsync(new Book
            {
                Title = "Ancien Titre",
                Author = "Auteur Initial"
            });

            var updatedData = new Book
            {
                Title = "Nouveau Titre",
                Author = "Auteur Initial",
                Genre = "Science-Fiction"
            };

            // Act
            var result = await _service.UpdateAsync(book.Id, updatedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nouveau Titre", result.Title);
            Assert.Equal("Science-Fiction", result.Genre);
        }

        // -------------------------------------------------------
        // Scénario 6 : UpdateAsync retourne null pour ID inexistant
        // -------------------------------------------------------
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Act
            var result = await _service.UpdateAsync(999, new Book
            {
                Title = "N/A",
                Author = "N/A"
            });

            // Assert
            Assert.Null(result);
        }

        // -------------------------------------------------------
        // Scénario 7 : DeleteAsync supprime le livre
        // -------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenBookExists()
        {
            // Arrange
            var book = await _service.CreateAsync(new Book
            {
                Title = "À supprimer",
                Author = "Auteur"
            });

            // Act
            var deleted = await _service.DeleteAsync(book.Id);

            // Assert
            Assert.True(deleted);

            // Vérifie que le livre n'existe plus
            var found = await _service.GetByIdAsync(book.Id);
            Assert.Null(found);
        }

        // -------------------------------------------------------
        // Scénario 8 : DeleteAsync retourne false pour ID inexistant
        // -------------------------------------------------------
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenBookDoesNotExist()
        {
            // Act
            var result = await _service.DeleteAsync(9999);

            // Assert
            Assert.False(result);
        }

        // -------------------------------------------------------
        // Scénario 9 : SearchAsync filtre par titre
        // -------------------------------------------------------
        [Fact]
        public async Task SearchAsync_ShouldFindBooks_ByTitle()
        {
            // Arrange
            await _service.CreateAsync(new Book { Title = "Harry Potter", Author = "Rowling" });
            await _service.CreateAsync(new Book { Title = "Dune", Author = "Herbert" });

            // Act
            var results = await _service.SearchAsync("harry");

            // Assert
            Assert.Single(results);
            Assert.Equal("Harry Potter", results.First().Title);
        }

        // -------------------------------------------------------
        // Scénario 10 : ToggleAvailabilityAsync inverse la disponibilité
        // -------------------------------------------------------
        [Fact]
        public async Task ToggleAvailabilityAsync_ShouldInvertAvailability()
        {
            // Arrange : livre disponible
            var book = await _service.CreateAsync(new Book
            {
                Title = "Disponible",
                Author = "Auteur",
                IsAvailable = true
            });

            // Act : on le rend indisponible
            var toggled = await _service.ToggleAvailabilityAsync(book.Id);

            // Assert
            Assert.NotNull(toggled);
            Assert.False(toggled.IsAvailable); // Doit être passé à false

            // Act : on le rend de nouveau disponible
            var toggled2 = await _service.ToggleAvailabilityAsync(book.Id);
            Assert.True(toggled2!.IsAvailable);
        }

        // -------------------------------------------------------
        // Cleanup : suppression de la base InMemory après chaque test
        // -------------------------------------------------------
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}