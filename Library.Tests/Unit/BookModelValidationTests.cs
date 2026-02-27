// =============================================================
// Fichier : Library.Tests/Unit/BookModelValidationTests.cs
// Rôle    : Tests unitaires des annotations de validation du modèle Book.
//           Vérifie que les DataAnnotations ([Required], [StringLength],
//           [Range]) fonctionnent correctement côté serveur.
// =============================================================

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Library.Shared.Models;

namespace Library.Tests.Unit
{
    /// <summary>
    /// Tests de validation du modèle Book.
    /// Utilise Validator.TryValidateObject pour simuler
    /// ce que fait ASP.NET Core avec ModelState.
    /// </summary>
    public class BookModelValidationTests
    {
        // -------------------------------------------------------
        // Méthode utilitaire : valide un objet et retourne les erreurs
        // -------------------------------------------------------
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        // -------------------------------------------------------
        // Test 1 : Livre valide → aucune erreur
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Livre valide ne génère aucune erreur")]
        public void Book_WithAllRequiredFields_IsValid()
        {
            // Arrange
            var book = new Book
            {
                Title = "Le Seigneur des Anneaux",
                Author = "J.R.R. Tolkien",
                PublishedYear = 1954
            };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().BeEmpty();
        }

        // -------------------------------------------------------
        // Test 2 : Titre manquant → erreur de validation
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Titre vide génère une erreur [Required]")]
        public void Book_WithEmptyTitle_FailsValidation()
        {
            // Arrange
            var book = new Book { Title = "", Author = "Auteur Valide" };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().ContainSingle(e =>
                e.MemberNames.Contains("Title"));
        }

        // -------------------------------------------------------
        // Test 3 : Auteur manquant → erreur de validation
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Auteur vide génère une erreur [Required]")]
        public void Book_WithEmptyAuthor_FailsValidation()
        {
            // Arrange
            var book = new Book { Title = "Titre Valide", Author = "" };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().ContainSingle(e =>
                e.MemberNames.Contains("Author"));
        }

        // -------------------------------------------------------
        // Test 4 : Titre trop long → erreur StringLength
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Titre > 200 caractères génère une erreur [StringLength]")]
        public void Book_WithTitleTooLong_FailsValidation()
        {
            // Arrange — titre de 201 caractères
            var book = new Book
            {
                Title = new string('A', 201),
                Author = "Auteur"
            };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().ContainSingle(e =>
                e.MemberNames.Contains("Title"));
        }

        // -------------------------------------------------------
        // Test 5 : Année valide → OK
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Année dans la plage [1000-2100] est valide")]
        public void Book_WithValidYear_IsValid()
        {
            // Arrange
            var book = new Book { Title = "Titre", Author = "Auteur", PublishedYear = 2023 };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().BeEmpty();
        }

        // -------------------------------------------------------
        // Test 6 : Année invalide → erreur Range
        // -------------------------------------------------------
        [Theory(DisplayName = "Modèle — Année hors plage génère une erreur [Range]")]
        [InlineData(999)]   // Trop tôt
        [InlineData(2101)]  // Trop tard
        [InlineData(0)]     // Zéro
        public void Book_WithInvalidYear_FailsValidation(int invalidYear)
        {
            // Arrange
            var book = new Book
            {
                Title = "Titre",
                Author = "Auteur",
                PublishedYear = invalidYear
            };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().ContainSingle(e =>
                e.MemberNames.Contains("PublishedYear"));
        }

        // -------------------------------------------------------
        // Test 7 : Champs optionnels peuvent être null
        // -------------------------------------------------------
        [Fact(DisplayName = "Modèle — Champs optionnels (ISBN, Genre, etc.) peuvent être null")]
        public void Book_WithNullOptionalFields_IsValid()
        {
            // Arrange
            var book = new Book
            {
                Title = "Titre Minimum",
                Author = "Auteur Minimum",
                ISBN = null,
                Genre = null,
                Description = null,
                CoverUrl = null,
                PublishedYear = null
            };

            // Act
            var errors = Validate(book);

            // Assert
            errors.Should().BeEmpty();
        }
    }
}