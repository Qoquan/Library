// =============================================================
// Fichier : Library.Tests/Unit/OpenLibraryServiceTests.cs
// Rôle    : Tests UNITAIRES du service OpenLibraryService.
//           Utilise Moq pour simuler HttpClient sans vrais appels réseau.
//           Teste la désérialisation JSON et la gestion d'erreurs.
// =============================================================

using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Library.API.Services;

namespace Library.Tests.Unit
{
    /// <summary>
    /// Tests unitaires pour OpenLibraryService.
    /// On ne fait PAS de vrais appels réseau : on mock HttpMessageHandler.
    /// Cela rend les tests rapides, fiables et indépendants d'internet.
    /// </summary>
    public class OpenLibraryServiceTests
    {
        // -------------------------------------------------------
        // Méthode utilitaire : crée un HttpClient mocké
        // qui retourne une réponse JSON précise
        // -------------------------------------------------------
        private static (HttpClient client, Mock<HttpMessageHandler> handlerMock)
            CreateMockedHttpClient(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://openlibrary.org")
            };

            return (client, handlerMock);
        }

        // -------------------------------------------------------
        // JSON de test simulant une réponse OpenLibrary valide
        // -------------------------------------------------------
        private const string ValidOpenLibraryJson = """
        {
          "numFound": 2,
          "docs": [
            {
              "title": "Dune",
              "author_name": ["Frank Herbert"],
              "isbn": ["9782207500477"],
              "first_publish_year": 1965,
              "subject": ["Science Fiction"],
              "cover_i": 12345
            },
            {
              "title": "Dune Messiah",
              "author_name": ["Frank Herbert"],
              "first_publish_year": 1969
            }
          ]
        }
        """;

        // -------------------------------------------------------
        // Test 1 : Désérialisation correcte
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Retourne les bons livres depuis le JSON")]
        public async Task SearchBooksAsync_WithValidResponse_ReturnsMappedBooks()
        {
            // Arrange
            var (client, _) = CreateMockedHttpClient(ValidOpenLibraryJson);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = (await service.SearchBooksAsync("dune")).ToList();

            // Assert
            results.Should().HaveCount(2);
            results[0].Title.Should().Be("Dune");
            results[0].Author.Should().Be("Frank Herbert");
            results[0].ISBN.Should().Be("9782207500477");
            results[0].PublishedYear.Should().Be(1965);
            results[0].Source.Should().Be("openlibrary");
        }

        // -------------------------------------------------------
        // Test 2 : URL de couverture construite depuis cover_i
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Construit l'URL de couverture depuis cover_i")]
        public async Task SearchBooksAsync_WithCoverId_BuildsCoverUrl()
        {
            // Arrange
            var (client, _) = CreateMockedHttpClient(ValidOpenLibraryJson);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = (await service.SearchBooksAsync("dune")).ToList();

            // Assert — le premier livre a cover_i = 12345
            results[0].CoverUrl.Should().Be("https://covers.openlibrary.org/b/id/12345-M.jpg");
        }

        // -------------------------------------------------------
        // Test 3 : Pas de couverture si cover_i absent
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — CoverUrl est null si pas de cover_i")]
        public async Task SearchBooksAsync_WithoutCoverId_CoverUrlIsNull()
        {
            // Arrange
            var (client, _) = CreateMockedHttpClient(ValidOpenLibraryJson);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = (await service.SearchBooksAsync("dune")).ToList();

            // Assert — le deuxième livre n'a pas de cover_i
            results[1].CoverUrl.Should().BeNull();
        }

        // -------------------------------------------------------
        // Test 4 : Réponse vide → liste vide (pas d'exception)
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Retourne liste vide si numFound = 0")]
        public async Task SearchBooksAsync_WithEmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var emptyJson = """{ "numFound": 0, "docs": [] }""";
            var (client, _) = CreateMockedHttpClient(emptyJson);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = await service.SearchBooksAsync("zzz_aucun_résultat");

            // Assert
            results.Should().BeEmpty();
        }

        // -------------------------------------------------------
        // Test 5 : Erreur réseau → liste vide (pas d'exception)
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Retourne liste vide en cas d'erreur réseau")]
        public async Task SearchBooksAsync_OnNetworkError_ReturnsEmptyList()
        {
            // Arrange — simule une erreur 500
            var (client, _) = CreateMockedHttpClient("{}", HttpStatusCode.InternalServerError);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = await service.SearchBooksAsync("dune");

            // Assert — ne lève pas d'exception, retourne vide
            results.Should().BeEmpty();
        }

        // -------------------------------------------------------
        // Test 6 : JSON malformé → liste vide (gestion d'erreur)
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Retourne liste vide si JSON invalide")]
        public async Task SearchBooksAsync_WithInvalidJson_ReturnsEmptyList()
        {
            // Arrange
            var (client, _) = CreateMockedHttpClient("NOT_VALID_JSON!!!");
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = await service.SearchBooksAsync("dune");

            // Assert
            results.Should().BeEmpty();
        }

        // -------------------------------------------------------
        // Test 7 : Auteur absent → "Inconnu" par défaut
        // -------------------------------------------------------
        [Fact(DisplayName = "OpenLibrary — Auteur 'Inconnu' si author_name absent")]
        public async Task SearchBooksAsync_WithNoAuthor_SetsAuthorToInconnu()
        {
            // Arrange
            var jsonNoAuthor = """
            {
              "numFound": 1,
              "docs": [{ "title": "Livre Mystère" }]
            }
            """;
            var (client, _) = CreateMockedHttpClient(jsonNoAuthor);
            var service = new OpenLibraryService(client, NullLogger<OpenLibraryService>.Instance);

            // Act
            var results = (await service.SearchBooksAsync("mystère")).ToList();

            // Assert
            results[0].Author.Should().Be("Inconnu");
        }
    }
}