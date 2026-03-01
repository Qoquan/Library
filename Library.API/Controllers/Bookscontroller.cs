// =============================================================
// Fichier : Library.API/Controllers/BooksController.cs
// Rôle    : Contrôleur REST pour l'API INTERNE des livres.
//           Expose les endpoints CRUD : GET, POST, PUT, DELETE.
//           POO : héritage de ControllerBase, injection de services.
// =============================================================

using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Controllers
{
    /// <summary>
    /// Contrôleur gérant les livres en base de données locale.
    /// Expose les opérations CRUD via l'API REST interne.
    /// Route de base : /api/books
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        // -------------------------------------------------------
        // Dépendances injectées
        // -------------------------------------------------------
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        // -------------------------------------------------------
        // GET /api/books — Tous les livres
        // -------------------------------------------------------
        /// <summary>Récupère la liste de tous les livres.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Book>), 200)]
        public async Task<ActionResult<IEnumerable<Book>>> GetAll()
        {
            _logger.LogInformation("GET /api/books - Récupération de tous les livres.");
            var books = await _bookService.GetAllAsync();
            return Ok(books);
        }

        // -------------------------------------------------------
        // GET /api/books/{id} — Un livre par ID
        // -------------------------------------------------------
        /// <summary>Récupère un livre par son identifiant.</summary>
        /// <param name="id">Identifiant du livre.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Book), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Book>> GetById(int id)
        {
            _logger.LogInformation("GET /api/books/{Id}", id);
            var book = await _bookService.GetByIdAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Livre {Id} non trouvé.", id);
                return NotFound(new { message = $"Livre avec l'ID {id} introuvable." });
            }

            return Ok(book);
        }

        // -------------------------------------------------------
        // GET /api/books/search?q= — Recherche
        // -------------------------------------------------------
        /// <summary>Recherche des livres par titre, auteur ou genre.</summary>
        /// <param name="q">Terme de recherche.</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Book>), 200)]
        public async Task<ActionResult<IEnumerable<Book>>> Search([FromQuery] string q = "")
        {
            _logger.LogInformation("GET /api/books/search?q={Query}", q);
            var books = await _bookService.SearchAsync(q);
            return Ok(books);
        }

        // -------------------------------------------------------
        // POST /api/books — Créer un livre
        // -------------------------------------------------------
        /// <summary>Crée un nouveau livre dans la base de données.</summary>
        /// <param name="book">Données du livre à créer.</param>
        [HttpPost]
        [ProducesResponseType(typeof(Book), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Book>> Create([FromBody] Book book)
        {
            // Validation automatique via les DataAnnotations du modèle
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("POST /api/books - Création : {Title}", book.Title);
            var created = await _bookService.CreateAsync(book);

            // Retourne 201 Created avec l'URL du nouveau livre
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // -------------------------------------------------------
        // PUT /api/books/{id} — Mettre à jour un livre
        // -------------------------------------------------------
        /// <summary>Met à jour un livre existant.</summary>
        /// <param name="id">Identifiant du livre.</param>
        /// <param name="book">Nouvelles données du livre.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Book), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Book>> Update(int id, [FromBody] Book book)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("PUT /api/books/{Id}", id);
            var updated = await _bookService.UpdateAsync(id, book);

            if (updated == null)
                return NotFound(new { message = $"Livre avec l'ID {id} introuvable." });

            return Ok(updated);
        }

        // -------------------------------------------------------
        // DELETE /api/books/{id} — Supprimer un livre
        // -------------------------------------------------------
        /// <summary>Supprime un livre de la base de données.</summary>
        /// <param name="id">Identifiant du livre.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("DELETE /api/books/{Id}", id);
            var deleted = await _bookService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Livre avec l'ID {id} introuvable." });

            // 204 No Content : suppression réussie, rien à retourner
            return NoContent();
        }

        // -------------------------------------------------------
        // PATCH /api/books/{id}/toggle — Changer la disponibilité
        // -------------------------------------------------------
        /// <summary>Inverse la disponibilité d'un livre (disponible ↔ emprunté).</summary>
        /// <param name="id">Identifiant du livre.</param>
        [HttpPatch("{id:int}/toggle")]
        [ProducesResponseType(typeof(Book), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Book>> ToggleAvailability(int id)
        {
            _logger.LogInformation("PATCH /api/books/{Id}/toggle", id);
            var book = await _bookService.ToggleAvailabilityAsync(id);

            if (book == null)
                return NotFound(new { message = $"Livre avec l'ID {id} introuvable." });

            return Ok(book);
        }
    }
}