// =============================================================
// Fichier : Library.Tests/Integration/BooksControllerIntegrationTests.cs
// Rôle    : Tests d'INTÉGRATION de l'API BooksController.
//           Teste le pipeline HTTP complet :
//           Client HTTP → Middleware → Controller → Service → BD InMemory
//
// Différence avec les tests unitaires :
//   Tests unitaires  → testent une classe isolée (mock des dépendances)
//   Tests intégration → testent plusieurs couches ensemble (vrai HTTP)
// =============================================================

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Library.Shared.Models;

namespace Library.Tests.Integration
{
    /// <summary>
    /// Tests d'intégration pour les endpoints /api/books.
    /// Chaque test effectue de vraies requêtes HTTP contre l'API
    /// (mais sans réseau réel grâce à WebApplicationFactory).
    ///
    /// IClassFixture = la factory est créée une seule fois pour toute la classe.
    /// </summary>
    public class BooksControllerIntegrationTests
        : IClassFixture<LibraryWebFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly LibraryWebFactory _factory;

        // Options JSON pour la désérialisation (insensible à la casse)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BooksControllerIntegrationTests(LibraryWebFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // ===============================================================
        // RÉGION : GET /api/books
        // ===============================================================
        #region GET - Liste

        [Fact(DisplayName = "GET /api/books → 200 OK avec tous les livres")]
        public async Task GetAll_ReturnsOkWithBooks()
        {
            // Act
            var response = await _client.GetAsync("/api/books");

            // Assert — statut HTTP
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert — corps de la réponse
            var books = await response.Content.ReadFromJsonAsync<List<Book>>(_jsonOptions);
            books.Should().NotBeNull();
            books!.Count.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "GET /api/books → Content-Type est application/json")]
        public async Task GetAll_ReturnsJsonContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/books");

            // Assert
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        [Fact(DisplayName = "GET /api/books → Les livres ont un ID > 0")]
        public async Task GetAll_BooksHaveValidIds()
        {
            // Act
            var books = await _client.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);

            // Assert
            books!.Should().AllSatisfy(b => b.Id.Should().BeGreaterThan(0));
        }

        #endregion

        // ===============================================================
        // RÉGION : GET /api/books/{id}
        // ===============================================================
        #region GET - Par ID

        [Fact(DisplayName = "GET /api/books/1 → 200 OK avec le livre correct")]
        public async Task GetById_ExistingBook_ReturnsOk()
        {
            // Arrange — récupère d'abord tous les livres pour avoir un ID valide
            var allBooks = await _client.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);
            var firstBookId = allBooks!.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/books/{firstBookId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var book = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            book!.Id.Should().Be(firstBookId);
        }

        [Fact(DisplayName = "GET /api/books/9999 → 404 Not Found")]
        public async Task GetById_NonExistingBook_Returns404()
        {
            // Act
            var response = await _client.GetAsync("/api/books/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        // ===============================================================
        // RÉGION : GET /api/books/search
        // ===============================================================
        #region GET - Recherche

        [Fact(DisplayName = "GET /api/books/search?q=dune → retourne les livres correspondants")]
        public async Task Search_WithValidQuery_ReturnsMatchingBooks()
        {
            // Act
            var books = await _client.GetFromJsonAsync<List<Book>>(
                "/api/books/search?q=dune", _jsonOptions);

            // Assert
            books.Should().NotBeNull();
            books!.Should().AllSatisfy(b =>
                b.Title.ToLower().Should().Contain("dune"));
        }

        [Fact(DisplayName = "GET /api/books/search?q= → 200 OK avec tous les livres")]
        public async Task Search_WithEmptyQuery_ReturnsAllBooks()
        {
            // Act
            var response = await _client.GetAsync("/api/books/search?q=");
            var books = await response.Content.ReadFromJsonAsync<List<Book>>(_jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            books!.Count.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "GET /api/books/search?q=zzz → liste vide")]
        public async Task Search_WithNoResults_ReturnsEmptyList()
        {
            // Act
            var books = await _client.GetFromJsonAsync<List<Book>>(
                "/api/books/search?q=xxxxxxxxxxxxxxxxxxx", _jsonOptions);

            // Assert
            books.Should().BeEmpty();
        }

        #endregion

        // ===============================================================
        // RÉGION : POST /api/books (Création)
        // ===============================================================
        #region POST - Création

        [Fact(DisplayName = "POST /api/books → 201 Created avec le livre créé")]
        public async Task Create_ValidBook_Returns201WithBook()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "Fondation",
                Author = "Isaac Asimov",
                Genre = "Science-Fiction",
                PublishedYear = 1951,
                IsAvailable = true
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/books", newBook);

            // Assert — statut 201
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Assert — corps
            var created = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            created!.Id.Should().BeGreaterThan(0);
            created.Title.Should().Be("Fondation");

            // Assert — header Location présent (bonne pratique REST)
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact(DisplayName = "POST /api/books sans titre → 400 Bad Request")]
        public async Task Create_MissingTitle_Returns400()
        {
            // Arrange — livre sans titre (validation échoue)
            var invalidBook = new { Author = "Auteur Valide" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/books", invalidBook);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "POST /api/books → livre disponible dans GET ensuite")]
        public async Task Create_BookIsRetrievableAfterCreation()
        {
            // Arrange
            var newBook = new Book { Title = "Test Persistance", Author = "Auteur Test" };

            // Act — créer
            var createResponse = await _client.PostAsJsonAsync("/api/books", newBook);
            var created = await createResponse.Content.ReadFromJsonAsync<Book>(_jsonOptions);

            // Assert — récupérer via GET
            var getResponse = await _client.GetAsync($"/api/books/{created!.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetched = await getResponse.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            fetched!.Title.Should().Be("Test Persistance");
        }

        #endregion

        // ===============================================================
        // RÉGION : PUT /api/books/{id} (Mise à jour)
        // ===============================================================
        #region PUT - Mise à jour

        [Fact(DisplayName = "PUT /api/books/{id} → 200 OK avec données mises à jour")]
        public async Task Update_ExistingBook_Returns200WithUpdatedData()
        {
            // Arrange — créer un livre à modifier
            var book = new Book { Title = "Avant MAJ", Author = "Auteur" };
            var createResp = await _client.PostAsJsonAsync("/api/books", book);
            var created = await createResp.Content.ReadFromJsonAsync<Book>(_jsonOptions);

            var updatedBook = new Book { Title = "Après MAJ", Author = "Auteur", Genre = "Nouveau Genre" };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/books/{created!.Id}", updatedBook);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            updated!.Title.Should().Be("Après MAJ");
            updated.Genre.Should().Be("Nouveau Genre");
        }

        [Fact(DisplayName = "PUT /api/books/9999 → 404 Not Found")]
        public async Task Update_NonExistingBook_Returns404()
        {
            // Arrange
            var book = new Book { Title = "Fantôme", Author = "Fantôme" };

            // Act
            var response = await _client.PutAsJsonAsync("/api/books/9999", book);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        // ===============================================================
        // RÉGION : DELETE /api/books/{id}
        // ===============================================================
        #region DELETE - Suppression

        [Fact(DisplayName = "DELETE /api/books/{id} → 204 No Content")]
        public async Task Delete_ExistingBook_Returns204()
        {
            // Arrange — créer un livre à supprimer
            var book = new Book { Title = "À Supprimer", Author = "Auteur" };
            var createResp = await _client.PostAsJsonAsync("/api/books", book);
            var created = await createResp.Content.ReadFromJsonAsync<Book>(_jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/books/{created!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "DELETE puis GET → 404 Not Found")]
        public async Task Delete_ThenGetSameBook_Returns404()
        {
            // Arrange — créer
            var book = new Book { Title = "Suppression Test", Author = "Auteur" };
            var createResp = await _client.PostAsJsonAsync("/api/books", book);
            var created = await createResp.Content.ReadFromJsonAsync<Book>(_jsonOptions);

            // Act — supprimer
            await _client.DeleteAsync($"/api/books/{created!.Id}");

            // Assert — plus accessible
            var getResp = await _client.GetAsync($"/api/books/{created.Id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE /api/books/9999 → 404 Not Found")]
        public async Task Delete_NonExistingBook_Returns404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/books/9999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        // ===============================================================
        // RÉGION : PATCH /api/books/{id}/toggle
        // ===============================================================
        #region PATCH - Toggle disponibilité

        [Fact(DisplayName = "PATCH /api/books/{id}/toggle → 200 OK avec état inversé")]
        public async Task Toggle_ExistingBook_Returns200WithToggledState()
        {
            // Arrange — créer un livre disponible
            var book = new Book { Title = "Toggle Test", Author = "Auteur", IsAvailable = true };
            var createResp = await _client.PostAsJsonAsync("/api/books", book);
            var created = await createResp.Content.ReadFromJsonAsync<Book>(_jsonOptions);

            // Act
            var response = await _client.PatchAsync($"/api/books/{created!.Id}/toggle", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var toggled = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            toggled!.IsAvailable.Should().BeFalse(); // était true → devient false
        }

        #endregion

        // -------------------------------------------------------
        // Cleanup
        // -------------------------------------------------------
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}