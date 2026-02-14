using BibliothequePersonnelle.Core.DTOs;

namespace BibliothequePersonnelle.Client.Services;

public interface ILivreService
{
    Task<IEnumerable<LivreDto>> GetAllLivresAsync();
    Task<LivreDto?> GetLivreByIdAsync(int id);
    Task<LivreDto> CreateLivreAsync(CreateLivreDto createDto);
    Task<LivreDto?> UpdateLivreAsync(int id, UpdateLivreDto updateDto);
    Task DeleteLivreAsync(int id);
    Task<IEnumerable<LivreDto>> SearchAsync(string query);
}
