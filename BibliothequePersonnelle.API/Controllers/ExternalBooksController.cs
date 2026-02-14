using BibliothequePersonnelle.Core.DTOs;
using BibliothequePersonnelle.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliothequePersonnelle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalBooksController : ControllerBase
{
    private readonly IExternalBookService _externalBookService;

    public ExternalBooksController(IExternalBookService externalBookService)
    {
        _externalBookService = externalBookService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ExternalBookDto>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter is required");

        var books = await _externalBookService.SearchBooksAsync(query);
        return Ok(books);
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<ActionResult<ExternalBookDto>> GetByISBN(string isbn)
    {
        var book = await _externalBookService.GetBookByISBNAsync(isbn);
        if (book == null)
            return NotFound();

        return Ok(book);
    }
}