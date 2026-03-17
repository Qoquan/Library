// =============================================================
// Fichier : Library.Tests/Integration/BooksControllerIntegrationTests.cs
// Rôle    : Tests d'intégration pour BooksController.
//           Chaque requête inclut le header X-User-Id pour
//           simuler un utilisateur connecté.
// =============================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Library.Shared.Models;

namespace Library.Tests.Integration
{
    public class BooksControllerIntegrationTests
        : IClassFixture<LibraryWebFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly LibraryWebFactory _factory;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        { PropertyNameCaseInsensitive = true };

        // UserId fictif pour les tests
        private const int TestUserId = 999;

        public BooksControllerIntegrationTests(LibraryWebFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            // Ajouter le header X-User-Id par défaut sur toutes les requêtes
            _client.DefaultRequestHeaders.Add("X-User-Id", TestUserId.ToString());
        }

        private static Book MakeBook(string title = "Test Book", string author = "Test Author") =>
            new Book { Title = title, Author = author, Genre = "Fiction", IsAvailable = true };

        // ── GET /api/books ─────────────────────────────────────────────

        [Fact(DisplayName = "GET /api/books → 200 OK avec tous les livres")]
        public async Task GetAll_ReturnsOkWithBooks()
        {
            var response = await _client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var books = await response.Content.ReadFromJsonAsync<List<Book>>(_jsonOptions);
            books.Should().NotBeNull();
        }

        [Fact(DisplayName = "GET /api/books → Content-Type est application/json")]
        public async Task GetAll_ReturnsJsonContentType()
        {
            var response = await _client.GetAsync("/api/books");
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        }

        [Fact(DisplayName = "GET /api/books sans header → 401 Unauthorized")]
        public async Task GetAll_WithoutUserId_Returns401()
        {
            var client = _factory.CreateClient(); // pas de header
            var response = await client.GetAsync("/api/books");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── POST /api/books ────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/books → 201 Created avec le livre créé")]
        public async Task Create_ReturnsCreated()
        {
            var book = MakeBook("Livre Intégration", "Auteur Test");
            var response = await _client.PostAsJsonAsync("/api/books", book);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            created!.Title.Should().Be("Livre Intégration");
            created.Id.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "POST /api/books sans titre → 400 Bad Request")]
        public async Task Create_WithoutTitle_ReturnsBadRequest()
        {
            var book = new Book { Title = "", Author = "Auteur" };
            var response = await _client.PostAsJsonAsync("/api/books", book);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "POST /api/books → livre disponible dans GET ensuite")]
        public async Task Create_ThenGetAll_ContainsNewBook()
        {
            var title = $"Livre_{Guid.NewGuid()}";
            await _client.PostAsJsonAsync("/api/books", MakeBook(title));
            var books = await _client.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);
            books!.Should().Contain(b => b.Title == title);
        }

        // ── GET /api/books/{id} ────────────────────────────────────────

        [Fact(DisplayName = "GET /api/books/9999 → 404 Not Found")]
        public async Task GetById_NotFound_Returns404()
        {
            var response = await _client.GetAsync("/api/books/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "GET /api/books/1 → 200 OK avec le livre correct")]
        public async Task GetById_AfterCreate_ReturnsCorrectBook()
        {
            var created = await (await _client.PostAsJsonAsync("/api/books", MakeBook("Livre GetById")))
                .Content.ReadFromJsonAsync<Book>(_jsonOptions);
            var response = await _client.GetAsync($"/api/books/{created!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var fetched = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            fetched!.Title.Should().Be("Livre GetById");
        }

        // ── PUT /api/books/{id} ────────────────────────────────────────

        [Fact(DisplayName = "PUT /api/books/{id} → 200 OK avec données mises à jour")]
        public async Task Update_ReturnsUpdatedBook()
        {
            var created = await (await _client.PostAsJsonAsync("/api/books", MakeBook("Avant")))
                .Content.ReadFromJsonAsync<Book>(_jsonOptions);
            var updated = created! with { Title = "Après" };
            var response = await _client.PutAsJsonAsync($"/api/books/{created.Id}", updated);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            result!.Title.Should().Be("Après");
        }

        [Fact(DisplayName = "PUT /api/books/9999 → 404 Not Found")]
        public async Task Update_NotFound_Returns404()
        {
            var response = await _client.PutAsJsonAsync("/api/books/9999", MakeBook());
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── DELETE /api/books/{id} ─────────────────────────────────────

        [Fact(DisplayName = "DELETE /api/books/{id} → 204 No Content")]
        public async Task Delete_ReturnsNoContent()
        {
            var created = await (await _client.PostAsJsonAsync("/api/books", MakeBook("À Supprimer")))
                .Content.ReadFromJsonAsync<Book>(_jsonOptions);
            var response = await _client.DeleteAsync($"/api/books/{created!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact(DisplayName = "DELETE /api/books/9999 → 404 Not Found")]
        public async Task Delete_NotFound_Returns404()
        {
            var response = await _client.DeleteAsync("/api/books/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE puis GET → 404 Not Found")]
        public async Task Delete_ThenGet_Returns404()
        {
            var created = await (await _client.PostAsJsonAsync("/api/books", MakeBook("Suppr puis Get")))
                .Content.ReadFromJsonAsync<Book>(_jsonOptions);
            await _client.DeleteAsync($"/api/books/{created!.Id}");
            var response = await _client.GetAsync($"/api/books/{created.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── PATCH /toggle ──────────────────────────────────────────────

        [Fact(DisplayName = "PATCH /api/books/{id}/toggle → 200 OK avec état inversé")]
        public async Task Toggle_InvertsAvailability()
        {
            var created = await (await _client.PostAsJsonAsync("/api/books", MakeBook("Toggle Test")))
                .Content.ReadFromJsonAsync<Book>(_jsonOptions);
            var original = created!.IsAvailable;
            var response = await _client.PatchAsync($"/api/books/{created.Id}/toggle", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var toggled = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            toggled!.IsAvailable.Should().Be(!original);
        }

        // ── GET /api/books/search ──────────────────────────────────────

        [Fact(DisplayName = "GET /api/books/search?q= → 200 OK avec tous les livres")]
        public async Task Search_EmptyQuery_ReturnsAll()
        {
            await _client.PostAsJsonAsync("/api/books", MakeBook("Recherche Vide"));
            var response = await _client.GetAsync("/api/books/search?q=");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "GET /api/books/search?q=dune → retourne les livres correspondants")]
        public async Task Search_WithValidQuery_ReturnsMatchingBooks()
        {
            // Crée un livre avec "dune" dans le titre pour cet utilisateur
            await _client.PostAsJsonAsync("/api/books",
                new Book { Title = "Dune", Author = "Frank Herbert", IsAvailable = true });

            var response = await _client.GetAsync("/api/books/search?q=dune");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var books = await response.Content.ReadFromJsonAsync<List<Book>>(_jsonOptions);
            books!.Should().NotBeEmpty();
            books!.Should().AllSatisfy(b =>
                (b.Title + b.Author + (b.Genre ?? "")).ToLower().Should().Contain("dune"));
        }

        [Fact(DisplayName = "GET /api/books/search?q=zzz → liste vide")]
        public async Task Search_WithNoMatch_ReturnsEmpty()
        {
            var books = await _client.GetFromJsonAsync<List<Book>>(
                "/api/books/search?q=zzzzzzzzzzz", _jsonOptions);
            books!.Should().BeEmpty();
        }

        // ── Isolation par utilisateur ──────────────────────────────────

        [Fact(DisplayName = "Deux utilisateurs ont des bibliothèques séparées")]
        public async Task TwoUsers_HaveSeparateLibraries()
        {
            // User 1 crée un livre
            var client1 = _factory.CreateClient();
            client1.DefaultRequestHeaders.Add("X-User-Id", "1001");
            await client1.PostAsJsonAsync("/api/books",
                new Book { Title = "Livre User 1", Author = "Auteur 1", IsAvailable = true });

            // User 2 crée un livre différent
            var client2 = _factory.CreateClient();
            client2.DefaultRequestHeaders.Add("X-User-Id", "1002");
            await client2.PostAsJsonAsync("/api/books",
                new Book { Title = "Livre User 2", Author = "Auteur 2", IsAvailable = true });

            // User 1 ne voit pas le livre de User 2
            var books1 = await client1.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);
            var books2 = await client2.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);

            books1!.Should().NotContain(b => b.Title == "Livre User 2");
            books2!.Should().NotContain(b => b.Title == "Livre User 1");
        }

        public void Dispose() { }
    }
}