// =============================================================
// Fichier : Library.API/Services/UserBookStore.cs
// Rôle    : Stockage en mémoire des livres PAR UTILISATEUR.
//           Chaque utilisateur possède sa propre liste de livres.
//           Singleton : données partagées entre toutes les requêtes
//           mais isolées par userId.
// POO     : Encapsulation, responsabilité unique.
// =============================================================

using Library.Shared.Models;

namespace Library.API.Services
{
    /// <summary>
    /// Interface du store de livres par utilisateur.
    /// </summary>
    public interface IUserBookStore
    {
        IEnumerable<Book> GetAll(int userId);
        Book? GetById(int userId, int bookId);
        IEnumerable<Book> Search(int userId, string query);
        Book Create(int userId, Book book);
        Book? Update(int userId, int bookId, Book book);
        bool Delete(int userId, int bookId);
        Book? ToggleAvailability(int userId, int bookId);
    }

    /// <summary>
    /// Implémentation en mémoire du store de livres par utilisateur.
    /// Chaque entrée du dictionnaire correspond à la bibliothèque
    /// personnelle d'un utilisateur.
    /// </summary>
    public class UserBookStore : IUserBookStore
    {
        // userId → liste de livres de cet utilisateur
        private readonly Dictionary<int, List<Book>> _store = new();
        private int _nextBookId = 1;
        private readonly object _lock = new();

        // ── Helpers privés ─────────────────────────────────────
        private List<Book> GetOrCreate(int userId)
        {
            if (!_store.ContainsKey(userId))
                _store[userId] = new List<Book>();
            return _store[userId];
        }

        // ── Opérations CRUD ────────────────────────────────────
        public IEnumerable<Book> GetAll(int userId)
        {
            lock (_lock)
                return GetOrCreate(userId)
                    .OrderBy(b => b.Title)
                    .ToList();
        }

        public Book? GetById(int userId, int bookId)
        {
            lock (_lock)
                return GetOrCreate(userId).FirstOrDefault(b => b.Id == bookId);
        }

        public IEnumerable<Book> Search(int userId, string query)
        {
            lock (_lock)
            {
                var books = GetOrCreate(userId);

                if (string.IsNullOrWhiteSpace(query))
                    return books.OrderBy(b => b.Title).ToList();

                var q = query.ToLower();
                return books
                    .Where(b =>
                        b.Title.ToLower().Contains(q) ||
                        b.Author.ToLower().Contains(q) ||
                        (b.Genre != null && b.Genre.ToLower().Contains(q)) ||
                        (b.ISBN != null && b.ISBN.Contains(query)))
                    .OrderBy(b => b.Title)
                    .ToList();
            }
        }

        public Book Create(int userId, Book book)
        {
            lock (_lock)
            {
                book.Id = _nextBookId++;
                book.CreatedAt = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(book.Source))
                    book.Source = "local";

                GetOrCreate(userId).Add(book);
                return book;
            }
        }

        public Book? Update(int userId, int bookId, Book book)
        {
            lock (_lock)
            {
                var existing = GetOrCreate(userId).FirstOrDefault(b => b.Id == bookId);
                if (existing == null) return null;

                existing.Title        = book.Title;
                existing.Author       = book.Author;
                existing.ISBN         = book.ISBN;
                existing.PublishedYear = book.PublishedYear;
                existing.Genre        = book.Genre;
                existing.Description  = book.Description;
                existing.CoverUrl     = book.CoverUrl;
                existing.IsAvailable  = book.IsAvailable;
                return existing;
            }
        }

        public bool Delete(int userId, int bookId)
        {
            lock (_lock)
            {
                var books = GetOrCreate(userId);
                var book = books.FirstOrDefault(b => b.Id == bookId);
                if (book == null) return false;
                books.Remove(book);
                return true;
            }
        }

        public Book? ToggleAvailability(int userId, int bookId)
        {
            lock (_lock)
            {
                var book = GetOrCreate(userId).FirstOrDefault(b => b.Id == bookId);
                if (book == null) return null;
                book.IsAvailable = !book.IsAvailable;
                return book;
            }
        }
    }
}