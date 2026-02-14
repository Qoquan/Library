using  BibliothequePersonnelle.Core.Entities;

namespace BibliothequePersonnelle.Core.Interfaces;

public interface ILivreRepository
{
    Task<IEnumerable<Livre>> GetAllAsync();
    Task<Livre?> GetByIdAsync(int id);
    Task<Livre> CreateAsync(Livre livre);
    Task UpdateAsync(int id, Livre livre);
    Task DeleteAsync(int id);
    Task<IEnumerable<Livre>> SearchAsync(string searchTerm );
    
}