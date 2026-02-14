using BibliothequePersonnelle.Core.Entities;
using BibliothequePersonnelle.Core.Interfaces;
using BibliothequePersonnelle.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BibliothequePersonnelle.Infrastructure.Repositories;

public class LivreRepository : ILivreRepository
{
    private readonly BibliothequeDbContext _context;

    public LivreRepository(BibliothequeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Livre>> GetAllAsync()
    {
        return await _context.Livres
            .OrderByDescending(l => l.DateAjout)
            .ToListAsync();
    }

    public async Task<Livre?> GetByIdAsync(int id)
    {
        return await _context.Livres.FindAsync(id);
    }

    public async Task<Livre> CreateAsync(Livre livre)
    {
        _context.Livres.Add(livre);
        await _context.SaveChangesAsync();
        return livre;
    }

    public async Task<Livre?> UpdateAsync(int id, Livre livre)
    {
        var existingLivre = await _context.Livres.FindAsync(id);
        if (existingLivre == null)
            return null;

        existingLivre.Titre = livre.Titre;
        existingLivre.Auteur = livre.Auteur;
        existingLivre.ISBN = livre.ISBN;
        existingLivre.DatePublication = livre.DatePublication;
        existingLivre.Description = livre.Description;
        existingLivre.ImageUrl = livre.ImageUrl;
        existingLivre.NombrePages = livre.NombrePages;
        existingLivre.Categorie = livre.Categorie;
        existingLivre.EstLu = livre.EstLu;
        existingLivre.Note = livre.Note;
        existingLivre.Commentaire = livre.Commentaire;

        await _context.SaveChangesAsync();
        return existingLivre;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var livre = await _context.Livres.FindAsync(id);
        if (livre == null)
            return false;

        _context.Livres.Remove(livre);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Livre>> SearchAsync(string searchTerm)
    {
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.Livres
            .Where(l => l.Titre.ToLower().Contains(lowerSearchTerm) ||
                       l.Auteur.ToLower().Contains(lowerSearchTerm) ||
                       (l.ISBN != null && l.ISBN.Contains(searchTerm)))
            .ToListAsync();
    }

    Task ILivreRepository.UpdateAsync(int id, Livre livre)
    {
        return UpdateAsync(id, livre);
    }

    Task ILivreRepository.DeleteAsync(int id)
    {
        return DeleteAsync(id);
    }
}