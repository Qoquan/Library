// =============================================================
// Fichier : Library.API/Services/IBookService.cs
// Rôle    : Interface définissant le contrat du service livre.
//           POO : abstraction via interface + injection de dépendances.
//           Facilite les tests unitaires (mock possible).
// =============================================================

using Library.Shared.Models;

namespace Library.API.Services
{
    /// <summary>
    /// Interface du service de gestion des livres (CRUD local).
    /// Définit toutes les opérations disponibles sur les livres.
    /// </summary>
    public interface IBookService
    {
        /// <summary>Récupère tous les livres.</summary>
        Task<IEnumerable<Book>> GetAllAsync();

        /// <summary>Récupère un livre par son identifiant.</summary>
        /// <param name="id">Identifiant du livre.</param>
        Task<Book?> GetByIdAsync(int id);

        /// <summary>Recherche des livres par titre ou auteur.</summary>
        /// <param name="query">Terme de recherche.</param>
        Task<IEnumerable<Book>> SearchAsync(string query);

        /// <summary>Crée un nouveau livre.</summary>
        /// <param name="book">Données du livre à créer.</param>
        Task<Book> CreateAsync(Book book);

        /// <summary>Met à jour un livre existant.</summary>
        /// <param name="id">Identifiant du livre.</param>
        /// <param name="book">Nouvelles données.</param>
        Task<Book?> UpdateAsync(int id, Book book);

        /// <summary>Supprime un livre par son identifiant.</summary>
        /// <param name="id">Identifiant du livre.</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>Change la disponibilité d'un livre.</summary>
        /// <param name="id">Identifiant du livre.</param>
        Task<Book?> ToggleAvailabilityAsync(int id);
    }
}