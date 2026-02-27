// =============================================================
// Fichier : Library.API/Services/BookService.cs
// Rôle    : Implémentation du service CRUD pour les livres.
//           Utilise Entity Framework Core pour accéder à SQLite.
//           POO : implémentation d'interface, injection de dépendances.
// =============================================================

using Microsoft.EntityFrameworkCore;
using Library.API.Data;
using Library.Shared.Models;

namespace Library.API.Services
{
    /// <summary>
    /// Service de gestion des livres en base de données locale (SQLite).
    /// Implémente l'interface IBookService.
    /// POO : encapsulation de la logique métier, séparation des responsabilités.
    /// </summary>
    public class BookService : IBookService
    {
        // -------------------------------------------------------
        // Dépendance injectée : contexte de base de données
        // -------------------------------------------------------
        private readonly LibraryDbContext _context;

        public BookService(LibraryDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------
        // READ - Lire tous les livres
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // -------------------------------------------------------
        // READ - Lire un livre par ID
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        // -------------------------------------------------------
        // READ - Rechercher des livres
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<IEnumerable<Book>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            var lowerQuery = query.ToLower();

            return await _context.Books
                .Where(b =>
                    b.Title.ToLower().Contains(lowerQuery) ||
                    b.Author.ToLower().Contains(lowerQuery) ||
                    (b.Genre != null && b.Genre.ToLower().Contains(lowerQuery)) ||
                    (b.ISBN != null && b.ISBN.Contains(query)))
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // -------------------------------------------------------
        // CREATE - Créer un livre
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<Book> CreateAsync(Book book)
        {
            // On s'assure que la date de création est définie
            book.CreatedAt = DateTime.UtcNow;
            book.Source = "local";

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return book;
        }

        // -------------------------------------------------------
        // UPDATE - Mettre à jour un livre
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<Book?> UpdateAsync(int id, Book book)
        {
            // Vérifie que le livre existe
            var existing = await _context.Books.FindAsync(id);
            if (existing == null) return null;

            // Mise à jour des propriétés (on ne change pas l'ID ni la date de création)
            existing.Title = book.Title;
            existing.Author = book.Author;
            existing.ISBN = book.ISBN;
            existing.PublishedYear = book.PublishedYear;
            existing.Genre = book.Genre;
            existing.Description = book.Description;
            existing.CoverUrl = book.CoverUrl;
            existing.IsAvailable = book.IsAvailable;

            await _context.SaveChangesAsync();

            return existing;
        }

        // -------------------------------------------------------
        // DELETE - Supprimer un livre
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return true;
        }

        // -------------------------------------------------------
        // TOGGLE - Changer la disponibilité
        // -------------------------------------------------------
        /// <inheritdoc />
        public async Task<Book?> ToggleAvailabilityAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return null;

            book.IsAvailable = !book.IsAvailable;
            await _context.SaveChangesAsync();

            return book;
        }
    }
}