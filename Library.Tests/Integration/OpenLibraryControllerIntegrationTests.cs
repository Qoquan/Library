// =============================================================
// Fichier : Library.Tests/Integration/OpenLibraryControllerIntegrationTests.cs
// Rôle    : Tests d'intégration pour les endpoints /api/openlibrary.
//           Teste le pipeline HTTP complet du contrôleur proxy.
//           Note : les appels vers OpenLibrary.org sont mockés via
//           la factory (pas de vraie connexion internet).
// =============================================================

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Library.Shared.Models;

namespace Library.Tests.Integration
{
    /// <summary>
    /// Tests d'intégration pour les endpoints OpenLibrary.
    /// Vérifie que les routes, validations et comportements
    /// du contrôleur proxy fonctionnent correctement.
    /// </summary>
    public class OpenLibraryControllerIntegrationTests
        : IClassFixture<LibraryWebFactory>, IDisposable
    {
        private readonly HttpClient _client;

        public OpenLibraryControllerIntegrationTests(LibraryWebFactory factory)
        {
            _client = factory.CreateClient();
        }

        // -------------------------------------------------------
        // Test 1 : Recherche sans paramètre → 400
        // -------------------------------------------------------
        [Fact(DisplayName = "GET /api/openlibrary/search sans q → 400 Bad Request")]
        public async Task SearchExternal_WithoutQuery_Returns400()
        {
            // Act
            var response = await _client.GetAsync("/api/openlibrary/search");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "GET /api/openlibrary/search?q= (vide) → 400 Bad Request")]
        public async Task SearchExternal_WithEmptyQuery_Returns400()
        {
            // Act
            var response = await _client.GetAsync("/api/openlibrary/search?q=");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // -------------------------------------------------------
        // Test 2 : Import valide → 201 Created
        // -------------------------------------------------------
        [Fact(DisplayName = "POST /api/openlibrary/import livre valide → 201 Created")]
        public async Task ImportBook_WithValidBook_Returns201()
        {
            // Arrange
            var book = new Book
            {
                Title = "Livre Importé depuis OpenLibrary",
                Author = "Auteur Externe",
                PublishedYear = 2020,
                Source = "openlibrary"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/openlibrary/import", book);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var imported = await response.Content.ReadFromJsonAsync<Book>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            imported!.Title.Should().Be("Livre Importé depuis OpenLibrary");
            imported.Source.Should().Be("openlibrary");
            imported.Id.Should().BeGreaterThan(0);
        }

        // -------------------------------------------------------
        // Test 3 : Import sans titre → 400
        // -------------------------------------------------------
        [Fact(DisplayName = "POST /api/openlibrary/import sans titre → 400 Bad Request")]
        public async Task ImportBook_WithEmptyTitle_Returns400()
        {
            // Arrange
            var invalidBook = new { Author = "Auteur" }; // pas de titre

            // Act
            var response = await _client.PostAsJsonAsync("/api/openlibrary/import", invalidBook);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // -------------------------------------------------------
        // Test 4 : Le livre importé est récupérable via /api/books
        // -------------------------------------------------------
        [Fact(DisplayName = "POST import → livre disponible dans /api/books")]
        public async Task ImportBook_IsRetrievableViaBooks()
        {
            // Arrange
            var book = new Book
            {
                Title = "Test Récupération Import",
                Author = "Auteur Import",
                Source = "openlibrary"
            };

            // Act — importer
            var importResp = await _client.PostAsJsonAsync("/api/openlibrary/import", book);
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            { PropertyNameCaseInsensitive = true };
            var imported = await importResp.Content.ReadFromJsonAsync<Book>(jsonOptions);

            // Assert — récupérable via l'endpoint books
            var getResp = await _client.GetAsync($"/api/books/{imported!.Id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetched = await getResp.Content.ReadFromJsonAsync<Book>(jsonOptions);
            fetched!.Title.Should().Be("Test Récupération Import");
            fetched.Source.Should().Be("openlibrary");
        }

        public void Dispose() => _client.Dispose();
    }
}