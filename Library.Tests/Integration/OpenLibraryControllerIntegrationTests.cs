// =============================================================
// Fichier : Library.Tests/Integration/OpenLibraryControllerIntegrationTests.cs
// =============================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Library.Shared.Models;

namespace Library.Tests.Integration
{
    public class OpenLibraryControllerIntegrationTests
        : IClassFixture<LibraryWebFactory>
    {
        private readonly HttpClient _client;
        private readonly LibraryWebFactory _factory;
        private const int TestUserId = 888;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        { PropertyNameCaseInsensitive = true };

        public OpenLibraryControllerIntegrationTests(LibraryWebFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("X-User-Id", TestUserId.ToString());
        }

        [Fact(DisplayName = "GET /api/openlibrary/search sans q → 400 Bad Request")]
        public async Task Search_WithoutQuery_Returns400()
        {
            var response = await _client.GetAsync("/api/openlibrary/search");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "GET /api/openlibrary/search?q= (vide) → 400 Bad Request")]
        public async Task Search_WithEmptyQuery_Returns400()
        {
            var response = await _client.GetAsync("/api/openlibrary/search?q=");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "POST /api/openlibrary/import livre valide → 201 Created")]
        public async Task ImportBook_WithValidBook_Returns201()
        {
            var book = new Book
            {
                Title = "Dune Importé",
                Author = "Frank Herbert",
                Source = "openlibrary",
                IsAvailable = true
            };
            var response = await _client.PostAsJsonAsync("/api/openlibrary/import", book);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var imported = await response.Content.ReadFromJsonAsync<Book>(_jsonOptions);
            imported!.Source.Should().Be("openlibrary");
            imported.Title.Should().Be("Dune Importé");
        }

        [Fact(DisplayName = "POST /api/openlibrary/import sans titre → 400 Bad Request")]
        public async Task ImportBook_WithoutTitle_Returns400()
        {
            var book = new Book { Title = "", Author = "Auteur" };
            var response = await _client.PostAsJsonAsync("/api/openlibrary/import", book);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "POST import → livre disponible dans /api/books")]
        public async Task ImportBook_IsRetrievableViaBooks()
        {
            var book = new Book
            {
                Title = $"Importé_{Guid.NewGuid()}",
                Author = "Auteur Import",
                Source = "openlibrary",
                IsAvailable = true
            };
            await _client.PostAsJsonAsync("/api/openlibrary/import", book);
            var books = await _client.GetFromJsonAsync<List<Book>>("/api/books", _jsonOptions);
            var fetched = books!.FirstOrDefault(b => b.Title == book.Title);
            fetched.Should().NotBeNull();
            fetched!.Source.Should().Be("openlibrary");
        }

        [Fact(DisplayName = "POST /api/openlibrary/import sans header → 401")]
        public async Task ImportBook_WithoutUserId_Returns401()
        {
            var client = _factory.CreateClient(); // pas de header
            var book = new Book { Title = "Test", Author = "Auteur", IsAvailable = true };
            var response = await client.PostAsJsonAsync("/api/openlibrary/import", book);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}