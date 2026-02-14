namespace BibliothequePersonnelle.Core.Entities;

public class Livre
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public DateTime? DatePublication { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? NombrePages { get; set; }
    public string? Categorie { get; set; }
    public bool EstLu { get; set; }
    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
    public int? Note { get; set; }
    public string? Commentaire { get; set; }
}