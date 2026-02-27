// =============================================================
// Fichier : Library.Shared/Models/Book.cs
// Rôle    : Modèle principal représentant un livre.
//           Partagé entre l'API et le frontend Blazor.
// =============================================================

using System.ComponentModel.DataAnnotations;

namespace Library.Shared.Models
{
    /// <summary>
    /// Représente un livre dans la bibliothèque.
    /// C'est la classe centrale de l'application (POO : encapsulation des données).
    /// </summary>
    public class Book
    {
        /// <summary>Identifiant unique du livre (clé primaire).</summary>
        public int Id { get; set; }

        /// <summary>Titre du livre (obligatoire).</summary>
        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [StringLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Auteur du livre (obligatoire).</summary>
        [Required(ErrorMessage = "L'auteur est obligatoire.")]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        /// <summary>Code ISBN du livre (identifiant international).</summary>
        [StringLength(20)]
        public string? ISBN { get; set; }

        /// <summary>Année de publication.</summary>
        [Range(1000, 2100, ErrorMessage = "Année invalide.")]
        public int? PublishedYear { get; set; }

        /// <summary>Genre littéraire (Roman, Science, Histoire, etc.).</summary>
        public string? Genre { get; set; }

        /// <summary>Description ou résumé du livre.</summary>
        public string? Description { get; set; }

        /// <summary>URL de la couverture (peut venir de l'API OpenLibrary).</summary>
        public string? CoverUrl { get; set; }

        /// <summary>Indique si le livre est actuellement disponible.</summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>Date d'ajout dans le système.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Source du livre : "local" ou "openlibrary".</summary>
        public string Source { get; set; } = "local";
    }
}