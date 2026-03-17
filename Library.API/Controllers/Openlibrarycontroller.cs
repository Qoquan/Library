// =============================================================
// Fichier : Library.API/Controllers/OpenLibraryController.cs
// Rôle    : Proxy vers OpenLibrary + import dans la bibliothèque
//           personnelle de l'utilisateur (via X-User-Id).
// =============================================================

using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OpenLibraryController : ControllerBase
    {
        private readonly IOpenLibraryService _openLibraryService;
        private readonly IUserBookStore _bookStore;
        private readonly ILogger<OpenLibraryController> _logger;

        public OpenLibraryController(
            IOpenLibraryService openLibraryService,
            IUserBookStore bookStore,
            ILogger<OpenLibraryController> logger)
        {
            _openLibraryService = openLibraryService;
            _bookStore = bookStore;
            _logger = logger;
        }

        private int? GetUserId()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var val) &&
                int.TryParse(val, out var id))
                return id;
            return null;
        }

        // GET /api/openlibrary/search?q=
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Le terme de recherche est obligatoire." });

            limit = Math.Clamp(limit, 1, 20);
            _logger.LogInformation("Recherche OpenLibrary : '{Query}'", q);

            var books = await _openLibraryService.SearchBooksAsync(q, limit);
            return Ok(books);
        }

        // POST /api/openlibrary/import
        [HttpPost("import")]
        public IActionResult ImportBook([FromBody] Book book)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Vous devez être connecté." });

            if (book == null || string.IsNullOrWhiteSpace(book.Title))
                return BadRequest(new { message = "Données du livre invalides." });

            book.Source = "openlibrary";
            _logger.LogInformation("Import OpenLibrary pour user {UserId} : '{Title}'", userId, book.Title);

            var created = _bookStore.Create(userId.Value, book);
            return StatusCode(201, created);
        }
    }
}