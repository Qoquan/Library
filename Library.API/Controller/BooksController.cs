// =============================================================
// Fichier : Library.API/Controllers/OpenLibraryController.cs
// Rôle    : Contrôleur proxy pour l'API EXTERNE OpenLibrary.
//           Permet au frontend de chercher des livres en ligne
//           et de les importer dans la base de données locale.
//           Route de base : /api/openlibrary
// =============================================================

using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Controllers
{
    /// <summary>
    /// Contrôleur exposant les fonctionnalités de recherche externe (OpenLibrary).
    /// Agit comme un proxy entre le frontend et l'API publique OpenLibrary.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OpenLibraryController : ControllerBase
    {
        // -------------------------------------------------------
        // Dépendances injectées
        // -------------------------------------------------------
        private readonly IOpenLibraryService _openLibraryService;
        private readonly IBookService _bookService;
        private readonly ILogger<OpenLibraryController> _logger;

        public OpenLibraryController(
            IOpenLibraryService openLibraryService,
            IBookService bookService,
            ILogger<OpenLibraryController> logger)
        {
            _openLibraryService = openLibraryService;
            _bookService = bookService;
            _logger = logger;
        }

        // -------------------------------------------------------
        // GET /api/openlibrary/search?q= — Recherche externe
        // -------------------------------------------------------
        /// <summary>
        /// Recherche des livres dans l'API OpenLibrary.
        /// Retourne les résultats sans les sauvegarder localement.
        /// </summary>
        /// <param name="q">Terme de recherche.</param>
        /// <param name="limit">Nombre de résultats (défaut 10, max 20).</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<Book>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<Book>>> Search(
            [FromQuery] string q,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Le terme de recherche est obligatoire." });

            // Limite le nombre de résultats pour ne pas surcharger l'API
            limit = Math.Clamp(limit, 1, 20);

            _logger.LogInformation("Recherche OpenLibrary : '{Query}' (limit={Limit})", q, limit);

            var books = await _openLibraryService.SearchBooksAsync(q, limit);
            return Ok(books);
        }

        // -------------------------------------------------------
        // POST /api/openlibrary/import — Importer un livre
        // -------------------------------------------------------
        /// <summary>
        /// Importe un livre trouvé sur OpenLibrary dans la base locale.
        /// Le livre envoyé dans le body est sauvegardé dans SQLite.
        /// </summary>
        /// <param name="book">Livre à importer (provenant d'OpenLibrary).</param>
        [HttpPost("import")]
        [ProducesResponseType(typeof(Book), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Book>> ImportBook([FromBody] Book book)
        {
            if (book == null || string.IsNullOrWhiteSpace(book.Title))
                return BadRequest(new { message = "Données du livre invalides." });

            // Marque la source comme OpenLibrary
            book.Source = "openlibrary";

            _logger.LogInformation("Import depuis OpenLibrary : '{Title}'", book.Title);

            var created = await _bookService.CreateAsync(book);
            return CreatedAtAction("GetById", "Books", new { id = created.Id }, created);
        }
    }
}