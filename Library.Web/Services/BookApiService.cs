// =============================================================
// Fichier : Library.Web/Services/BookApiService.cs
// Rôle    : Communique avec Library.API depuis Blazor.
//           Envoie automatiquement le header X-User-Id pour
//           que l'API sache quel utilisateur fait la requête.
// =============================================================

using System.Net.Http.Json;
using Library.Shared.Models;

namespace Library.Web.Services
{
    public class BookApiService
    {
        private readonly HttpClient _http;
        private readonly AuthService _auth;

        public BookApiService(HttpClient http, AuthService auth)
        {
            _http = http;
            _auth = auth;
        }

        // ── Helper : ajoute X-User-Id à chaque requête ────────
        private HttpRequestMessage WithUserId(HttpMethod method, string url)
        {
            var req = new HttpRequestMessage(method, url);
            if (_auth.CurrentUser != null)
                req.Headers.Add("X-User-Id", _auth.CurrentUser.Id.ToString());
            return req;
        }

        // GET /api/books
        public async Task<List<Book>> GetAllBooksAsync()
        {
            try
            {
                var req = WithUserId(HttpMethod.Get, "api/books");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return new();
                return await resp.Content.ReadFromJsonAsync<List<Book>>() ?? new();
            }
            catch { return new(); }
        }

        // GET /api/books/{id}
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            try
            {
                var req = WithUserId(HttpMethod.Get, $"api/books/{id}");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<Book>();
            }
            catch { return null; }
        }

        // GET /api/books/search?q=
        public async Task<List<Book>> SearchBooksAsync(string query)
        {
            try
            {
                var encoded = Uri.EscapeDataString(query);
                var req = WithUserId(HttpMethod.Get, $"api/books/search?q={encoded}");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return new();
                return await resp.Content.ReadFromJsonAsync<List<Book>>() ?? new();
            }
            catch { return new(); }
        }

        // POST /api/books
        public async Task<Book?> CreateBookAsync(Book book)
        {
            try
            {
                var req = WithUserId(HttpMethod.Post, "api/books");
                req.Content = JsonContent.Create(book);
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<Book>();
            }
            catch { return null; }
        }

        // PUT /api/books/{id}
        public async Task<Book?> UpdateBookAsync(int id, Book book)
        {
            try
            {
                var req = WithUserId(HttpMethod.Put, $"api/books/{id}");
                req.Content = JsonContent.Create(book);
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<Book>();
            }
            catch { return null; }
        }

        // DELETE /api/books/{id}
        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var req = WithUserId(HttpMethod.Delete, $"api/books/{id}");
                var resp = await _http.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // PATCH /api/books/{id}/toggle
        public async Task<Book?> ToggleAvailabilityAsync(int id)
        {
            try
            {
                var req = WithUserId(HttpMethod.Patch, $"api/books/{id}/toggle");
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<Book>();
            }
            catch { return null; }
        }

        // GET /api/openlibrary/search?q=
        public async Task<List<Book>> SearchOpenLibraryAsync(string query)
        {
            try
            {
                var encoded = Uri.EscapeDataString(query);
                var books = await _http.GetFromJsonAsync<List<Book>>(
                    $"api/openlibrary/search?q={encoded}&limit=10");
                return books ?? new();
            }
            catch { return new(); }
        }

        // POST /api/openlibrary/import
        public async Task<Book?> ImportFromOpenLibraryAsync(Book book)
        {
            try
            {
                var req = WithUserId(HttpMethod.Post, "api/openlibrary/import");
                req.Content = JsonContent.Create(book);
                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<Book>();
            }
            catch { return null; }
        }
    }
}