// =============================================================
// Fichier : Library.Tests/Unit/BookServiceTests.cs
// Rôle    : Tests UNITAIRES du service BookService.
//           Teste chaque méthode de façon isolée (InMemory DB).
//           AA2 — Critère 1 : Définir les scénarios de test
//           AA2 — Critère 2 : Implémenter des tests unitaires
//
// Convention de nommage des tests :
//   [Méthode]_[Condition]_[ResultatAttendu]
//
// Lancer les tests :
//   cd Library.Tests && dotnet test
// =============================================================

using FluentAssertions;
using Library.API.Services;
using Library.Tests.Helpers;
using Library.Shared.Models;

namespace Library.Tests.Unit
{
    /// <summary>
    /// Suite de tests unitaires pour BookService.
    ///
    /// ORGANISATION EN RÉGIONS :
    ///   - GetAll      : lecture de tous les livres
    ///   - GetById     : lecture par identifiant
    ///   - Search      : recherche par mots-clés
    ///   - Create      : création d'un livre
    ///   - Update      : modification d'un livre
    ///   - Delete      : suppression d'un livre
    ///   - Toggle      : changement de disponibilité
    /// </summary>
    public class BookServiceTests : IDisposable
    {
        // -------------------------------------------------------
        // Setup commun : contexte + service frais pour chaque test
        // -------------------------------------------------------
        private readonly Library.API.Data.LibraryDbContext _context;
        private readonly BookService _service;

        public BookServiceTests()
        {
            // BD InMemory isolée — chaque test repart de zéro
            _context = TestDbHelper.CreateSeededContext();
            _service = new BookService(_context);
        }

        // ===============================================================
        // RÉGION : GetAllAsync
        // ===============================================================
        #region GetAllAsync

        [Fact(DisplayName = "GetAll — Retourne tous les livres seedés")]
        public async Task GetAllAsync_WhenBooksExist_ReturnsAllBooks()
        {
            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4); // 4 livres dans le seed
        }

        [Fact(DisplayName = "GetAll — Livres triés par titre alphabétiquement")]
        public async Task GetAllAsync_ReturnsBooksSortedByTitle()
        {
            // Act
            var result = (await _service.GetAllAsync()).ToList();

            // Assert — les titres doivent être en ordre alphabétique
            result.Should().BeInAscendingOrder(b => b.Title);
        }

        [Fact(DisplayName = "GetAll — Retourne liste vide si aucun livre")]
        public async Task GetAllAsync_WhenNoBooksExist_ReturnsEmptyList()
        {
            // Arrange — contexte complètement vide
            using var emptyContext = TestDbHelper.CreateInMemoryContext();
            var emptyService = new BookService(emptyContext);

            // Act
            var result = await emptyService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        // ===============================================================
        // RÉGION : GetByIdAsync
        // ===============================================================
        #region GetByIdAsync

        [Fact(DisplayName = "GetById — Retourne le bon livre pour un ID existant")]
        public async Task GetByIdAsync_WhenBookExists_ReturnsCorrectBook()
        {
            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Title.Should().Be("Le Petit Prince");
            result.Author.Should().Be("Antoine de Saint-Exupéry");
        }

        [Fact(DisplayName = "GetById — Retourne null pour un ID inexistant")]
        public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _service.GetByIdAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "GetById — Retourne null pour ID négatif")]
        public async Task GetByIdAsync_WithNegativeId_ReturnsNull()
        {
            // Act
            var result = await _service.GetByIdAsync(-1);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // ===============================================================
        // RÉGION : SearchAsync
        // ===============================================================
        #region SearchAsync

        [Fact(DisplayName = "Search — Trouve un livre par titre (insensible à la casse)")]
        public async Task SearchAsync_ByTitle_ReturnsMatchingBooks()
        {
            // Act — recherche en minuscules
            var result = (await _service.SearchAsync("petit prince")).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Le Petit Prince");
        }

        [Fact(DisplayName = "Search — Trouve des livres par auteur")]
        public async Task SearchAsync_ByAuthor_ReturnsMatchingBooks()
        {
            // Act
            var result = (await _service.SearchAsync("orwell")).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Author.Should().Be("George Orwell");
        }

        [Fact(DisplayName = "Search — Trouve des livres par genre")]
        public async Task SearchAsync_ByGenre_ReturnsMatchingBooks()
        {
            // Act
            var result = (await _service.SearchAsync("dystopie")).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Genre.Should().Be("Dystopie");
        }

        [Fact(DisplayName = "Search — Retourne tous les livres si requête vide")]
        public async Task SearchAsync_WithEmptyQuery_ReturnsAllBooks()
        {
            // Act
            var result = await _service.SearchAsync("");

            // Assert
            result.Should().HaveCount(4);
        }

        [Fact(DisplayName = "Search — Retourne liste vide si aucun résultat")]
        public async Task SearchAsync_WithUnknownQuery_ReturnsEmptyList()
        {
            // Act
            var result = await _service.SearchAsync("zzz_inexistant_zzz");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact(DisplayName = "Search — Trouve par ISBN")]
        public async Task SearchAsync_ByISBN_ReturnsMatchingBook()
        {
            // Act
            var result = (await _service.SearchAsync("978-2-07-040850-4")).ToList();

            // Assert
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Le Petit Prince");
        }

        #endregion

        // ===============================================================
        // RÉGION : CreateAsync
        // ===============================================================
        #region CreateAsync

        [Fact(DisplayName = "Create — Crée un livre et lui assigne un ID")]
        public async Task CreateAsync_WithValidBook_AssignsIdAndReturnsBook()
        {
            // Arrange
            var newBook = TestDbHelper.CreateValidBook("Fondation", "Isaac Asimov");

            // Act
            var created = await _service.CreateAsync(newBook);

            // Assert
            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.Title.Should().Be("Fondation");
            created.Author.Should().Be("Isaac Asimov");
        }

        [Fact(DisplayName = "Create — La source est forcée à 'local'")]
        public async Task CreateAsync_AlwaysSetsSourceToLocal()
        {
            // Arrange — on essaie de mettre une autre source
            var book = TestDbHelper.CreateValidBook();
            book.Source = "tentative_externe";

            // Act
            var created = await _service.CreateAsync(book);

            // Assert
            created.Source.Should().Be("local");
        }

        [Fact(DisplayName = "Create — La date de création est définie automatiquement")]
        public async Task CreateAsync_SetsCreatedAtAutomatically()
        {
            // Arrange
            var book = TestDbHelper.CreateValidBook();
            var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var created = await _service.CreateAsync(book);

            // Assert
            created.CreatedAt.Should().BeAfter(beforeCreate);
            created.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        }

        [Fact(DisplayName = "Create — Le livre est bien persisté en base")]
        public async Task CreateAsync_PersistsBookInDatabase()
        {
            // Arrange
            var book = TestDbHelper.CreateValidBook("Neuromancer");

            // Act
            var created = await _service.CreateAsync(book);

            // Assert — vérifie via GetById que le livre existe vraiment
            var fetched = await _service.GetByIdAsync(created.Id);
            fetched.Should().NotBeNull();
            fetched!.Title.Should().Be("Neuromancer");
        }

        #endregion

        // ===============================================================
        // RÉGION : UpdateAsync
        // ===============================================================
        #region UpdateAsync

        [Fact(DisplayName = "Update — Met à jour le titre et le genre")]
        public async Task UpdateAsync_WithValidData_UpdatesBook()
        {
            // Arrange
            var updatedData = new Book
            {
                Title = "Nouveau Titre",
                Author = "Antoine de Saint-Exupéry", // auteur inchangé
                Genre = "Philosophie",
                IsAvailable = true
            };

            // Act — mise à jour du livre ID=1
            var result = await _service.UpdateAsync(1, updatedData);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Nouveau Titre");
            result.Genre.Should().Be("Philosophie");
        }

        [Fact(DisplayName = "Update — L'ID n'est pas modifié lors de la mise à jour")]
        public async Task UpdateAsync_PreservesOriginalId()
        {
            // Arrange
            var updatedData = new Book { Title = "Titre MAJ", Author = "Auteur MAJ" };

            // Act
            var result = await _service.UpdateAsync(1, updatedData);

            // Assert
            result!.Id.Should().Be(1); // L'ID reste 1
        }

        [Fact(DisplayName = "Update — Retourne null si livre inexistant")]
        public async Task UpdateAsync_WhenBookDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _service.UpdateAsync(9999, new Book
            {
                Title = "Fantôme",
                Author = "Fantôme"
            });

            // Assert
            result.Should().BeNull();
        }

        [Fact(DisplayName = "Update — La modification est persistée en base")]
        public async Task UpdateAsync_ChangeIsPersisted()
        {
            // Arrange
            var updatedData = new Book { Title = "Titre Persisté", Author = "George Orwell" };

            // Act
            await _service.UpdateAsync(2, updatedData);

            // Assert — relit depuis la BD
            var fetched = await _service.GetByIdAsync(2);
            fetched!.Title.Should().Be("Titre Persisté");
        }

        #endregion

        // ===============================================================
        // RÉGION : DeleteAsync
        // ===============================================================
        #region DeleteAsync

        [Fact(DisplayName = "Delete — Supprime le livre et retourne true")]
        public async Task DeleteAsync_WhenBookExists_ReturnsTrueAndRemovesBook()
        {
            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();

            // Vérifie que le livre n'existe plus
            var deleted = await _service.GetByIdAsync(1);
            deleted.Should().BeNull();
        }

        [Fact(DisplayName = "Delete — Retourne false si livre inexistant")]
        public async Task DeleteAsync_WhenBookDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _service.DeleteAsync(9999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact(DisplayName = "Delete — Les autres livres ne sont pas affectés")]
        public async Task DeleteAsync_DoesNotAffectOtherBooks()
        {
            // Arrange — compte avant suppression
            var countBefore = (await _service.GetAllAsync()).Count();

            // Act
            await _service.DeleteAsync(1);

            // Assert — il reste countBefore - 1 livres
            var countAfter = (await _service.GetAllAsync()).Count();
            countAfter.Should().Be(countBefore - 1);
        }

        #endregion

        // ===============================================================
        // RÉGION : ToggleAvailabilityAsync
        // ===============================================================
        #region ToggleAvailabilityAsync

        [Fact(DisplayName = "Toggle — Passe disponible → indisponible")]
        public async Task ToggleAvailabilityAsync_WhenAvailable_SetsUnavailable()
        {
            // Arrange — livre ID=1 est disponible (IsAvailable = true)
            var book = await _service.GetByIdAsync(1);
            book!.IsAvailable.Should().BeTrue();

            // Act
            var toggled = await _service.ToggleAvailabilityAsync(1);

            // Assert
            toggled.Should().NotBeNull();
            toggled!.IsAvailable.Should().BeFalse();
        }

        [Fact(DisplayName = "Toggle — Passe indisponible → disponible")]
        public async Task ToggleAvailabilityAsync_WhenUnavailable_SetsAvailable()
        {
            // Arrange — livre ID=3 est indisponible (IsAvailable = false)
            var book = await _service.GetByIdAsync(3);
            book!.IsAvailable.Should().BeFalse();

            // Act
            var toggled = await _service.ToggleAvailabilityAsync(3);

            // Assert
            toggled!.IsAvailable.Should().BeTrue();
        }

        [Fact(DisplayName = "Toggle — Double toggle remet à l'état initial")]
        public async Task ToggleAvailabilityAsync_DoubleToggle_RestoresOriginalState()
        {
            // Arrange
            var original = await _service.GetByIdAsync(1);
            var originalState = original!.IsAvailable;

            // Act — deux toggles successifs
            await _service.ToggleAvailabilityAsync(1);
            var afterSecondToggle = await _service.ToggleAvailabilityAsync(1);

            // Assert — revenu à l'état initial
            afterSecondToggle!.IsAvailable.Should().Be(originalState);
        }

        [Fact(DisplayName = "Toggle — Retourne null si livre inexistant")]
        public async Task ToggleAvailabilityAsync_WhenBookDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _service.ToggleAvailabilityAsync(9999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        // -------------------------------------------------------
        // Cleanup : dispose du contexte après chaque test
        // -------------------------------------------------------
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}