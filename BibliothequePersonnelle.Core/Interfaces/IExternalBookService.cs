using  BibliothequePersonnelle.Core.DTOs;

namespace BibliothequePersonnelle.Core.Interfaces;

public interface IExternalBookService
{
    Task<IEnumerable<ExternalBookDto>> SearchBooksAsync(string query);
    Task<ExternalBookDto?> GetBookByISBNAsync(string isbn);
}

public class ExternalBookDto
{
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime? DatePublication { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? NombrePages { get; set; }
    public string? Categorie { get; set; }
}
