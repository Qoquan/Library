// =============================================================
// Fichier : Library.API/Services/OpenLibraryService.cs
// Rôle    : Service d'appel à l'API EXTERNE OpenLibrary.
//           URL : https://openlibrary.org/search.json?q=...
//           POO : encapsulation des appels HTTP, injection HttpClient.
// =============================================================

using System.Text.Json;
using Library.Shared.Models;

namespace Library.API.Services
{
    /// <summary>
    /// Interface pour l'accès à l'API externe OpenLibrary.
    /// Permet de rechercher des livres via l'API publique gratuite.
    /// </summary>
    public interface IOpenLibraryService
    {
        /// <summary>Recherche des livres dans OpenLibrary.</summary>
        /// <param name="query">Terme de recherche.</param>
        /// <param name="limit">Nombre maximum de résultats (défaut : 10).</param>
        Task<IEnumerable<Book>> SearchBooksAsync(string query, int limit = 10);
    }

    /// <summary>
    /// Implémentation du service OpenLibrary.
    /// Effectue des appels HTTP GET vers l'API publique OpenLibrary
    /// et convertit les résultats en objets Book de notre application.
    /// </summary>
    public class OpenLibraryService : IOpenLibraryService
    {
        // -------------------------------------------------------
        // Dépendances injectées
        // -------------------------------------------------------
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenLibraryService> _logger;

        // URL de base de l'API OpenLibrary
        private const string BaseUrl = "https://openlibrary.org";

        // Options de désérialisation JSON (insensible à la casse)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LibraryApp/1.0");
            _logger = logger;
        }

        // -------------------------------------------------------
        // Recherche dans OpenLibrary
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<IEnumerable<Book>> SearchBooksAsync(string query, int limit = 10)
        {
            try
            {
                // Construction de l'URL de recherche
                var encodedQuery = Uri.EscapeDataString(query);
                var url = $"/search.json?q={encodedQuery}&limit={limit}&fields=title,author_name,isbn,first_publish_year,subject,cover_i";

                _logger.LogInformation("Appel API OpenLibrary : {Url}", url);

                // Appel HTTP GET
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Lecture et désérialisation du JSON
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenLibraryResponse>(json, _jsonOptions);

                if (result?.Docs == null)
                    return Enumerable.Empty<Book>();

                // Conversion des DTOs en objets Book
                var books = result.Docs
                    .Select(doc => doc.ToBook())
                    .ToList();

                _logger.LogInformation("OpenLibrary a retourné {Count} livres.", books.Count);

                return books;
            }
            catch (HttpRequestException ex)
            {
                // En cas d'erreur réseau, on log et retourne une liste vide
                _logger.LogError(ex, "Erreur lors de l'appel à OpenLibrary.");
                return Enumerable.Empty<Book>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erreur de désérialisation JSON depuis OpenLibrary.");
                return Enumerable.Empty<Book>();
            }
        }
    }
}