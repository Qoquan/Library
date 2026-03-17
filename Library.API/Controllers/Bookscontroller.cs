// =============================================================
// Fichier : Library.API/Controllers/BooksController.cs
// Rôle    : Contrôleur REST pour les livres.
//           Chaque opération est isolée par userId (header X-User-Id).
//           Les livres sont stockés en mémoire dans UserBookStore.
// =============================================================

using Microsoft.AspNetCore.Mvc;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly IUserBookStore _bookStore;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IUserBookStore bookStore, ILogger<BooksController> logger)
        {
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

        private IActionResult Unauthorized401() =>
            Unauthorized(new { message = "Vous devez être connecté." });

        [HttpGet]
        public IActionResult GetAll()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            return Ok(_bookStore.GetAll(userId.Value));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            var book = _bookStore.GetById(userId.Value, id);
            if (book == null) return NotFound(new { message = $"Livre {id} introuvable." });
            return Ok(book);
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string q = "")
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            return Ok(_bookStore.Search(userId.Value, q));
        }

        [HttpPost]
        public IActionResult Create([FromBody] Book book)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = _bookStore.Create(userId.Value, book);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Book book)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = _bookStore.Update(userId.Value, id, book);
            if (updated == null) return NotFound(new { message = $"Livre {id} introuvable." });
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            var deleted = _bookStore.Delete(userId.Value, id);
            if (!deleted) return NotFound(new { message = $"Livre {id} introuvable." });
            return NoContent();
        }

        [HttpPatch("{id:int}/toggle")]
        public IActionResult ToggleAvailability(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized401();
            var book = _bookStore.ToggleAvailability(userId.Value, id);
            if (book == null) return NotFound(new { message = $"Livre {id} introuvable." });
            return Ok(book);
        }
    }
}