// =============================================================
// Fichier : Library.Shared/Models/OpenLibraryBook.cs
// Rôle    : DTO (Data Transfer Object) pour désérialiser
//           les résultats de l'API externe OpenLibrary.
//           URL de l'API : https://openlibrary.org/search.json
// =============================================================

using System.Text.Json.Serialization;

namespace Library.Shared.Models
{
    /// <summary>
    /// Représente la réponse globale de l'API OpenLibrary.
    /// </summary>
    public class OpenLibraryResponse
    {
        /// <summary>Nombre total de résultats trouvés.</summary>
        [JsonPropertyName("numFound")]
        public int NumFound { get; set; }

        /// <summary>Liste des livres retournés par l'API.</summary>
        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc> Docs { get; set; } = new();
    }

    /// <summary>
    /// Représente un document (livre) retourné par OpenLibrary.
    /// Les noms de propriétés correspondent aux champs JSON de l'API.
    /// </summary>
    public class OpenLibraryDoc
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("author_name")]
        public List<string>? AuthorName { get; set; }

        [JsonPropertyName("isbn")]
        public List<string>? ISBN { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("subject")]
        public List<string>? Subjects { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverId { get; set; }

        /// <summary>
        /// Construit l'URL de couverture depuis l'identifiant OpenLibrary.
        /// </summary>
        public string? GetCoverUrl()
        {
            if (CoverId.HasValue)
                return $"https://covers.openlibrary.org/b/id/{CoverId}-M.jpg";
            return null;
        }

        /// <summary>
        /// Convertit ce DTO en objet Book utilisable dans l'application.
        /// Exemple de méthode de conversion (POO : responsabilité unique).
        /// </summary>
        public Book ToBook()
        {
            return new Book
            {
                Title = Title,
                Author = AuthorName?.FirstOrDefault() ?? "Inconnu",
                ISBN = ISBN?.FirstOrDefault(),
                PublishedYear = FirstPublishYear,
                Genre = Subjects?.FirstOrDefault(),
                CoverUrl = GetCoverUrl(),
                Source = "openlibrary",
                IsAvailable = true
            };
        }
    }
}