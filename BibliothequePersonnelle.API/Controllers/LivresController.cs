using BibliothequePersonnelle.Core.DTOs;
using BibliothequePersonnelle.Core.Entities;
using BibliothequePersonnelle.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliothequePersonnelle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivresController : ControllerBase
{
    private readonly ILivreRepository _repository;

    public LivresController(ILivreRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LivreDto>>> GetAll()
    {
        var livres = await _repository.GetAllAsync();
        var livresDto = livres.Select(MapToDto);
        return Ok(livresDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LivreDto>> GetById(int id)
    {
        var livre = await _repository.GetByIdAsync(id);
        if (livre == null)
            return NotFound();

        return Ok(MapToDto(livre));
    }

    [HttpPost]
    public async Task<ActionResult<LivreDto>> Create(CreateLivreDto createDto)
    {
        var livre = new Livre
        {
            Titre = createDto.Titre,
            Auteur = createDto.Auteur,
            ISBN = createDto.ISBN,
            DatePublication = createDto.DatePublication,
            Description = createDto.Description,
            ImageUrl = createDto.ImageUrl,
            NombrePages = createDto.NombrePages,
            Categorie = createDto.Categorie,
            DateAjout = DateTime.UtcNow
        };

        var createdLivre = await _repository.CreateAsync(livre);
        return CreatedAtAction(nameof(GetById), new { id = createdLivre.Id }, MapToDto(createdLivre));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LivreDto>> Update(int id, UpdateLivreDto updateDto)
    {
        var existingLivre = await _repository.GetByIdAsync(id);
        if (existingLivre == null)
            return NotFound();

        // Mise à jour des propriétés
        if (updateDto.Titre != null) existingLivre.Titre = updateDto.Titre;
        if (updateDto.Auteur != null) existingLivre.Auteur = updateDto.Auteur;
        if (updateDto.ISBN != null) existingLivre.ISBN = updateDto.ISBN;
        if (updateDto.DatePublication.HasValue) existingLivre.DatePublication = updateDto.DatePublication;
        if (updateDto.Description != null) existingLivre.Description = updateDto.Description;
        if (updateDto.ImageUrl != null) existingLivre.ImageUrl = updateDto.ImageUrl;
        if (updateDto.NombrePages.HasValue) existingLivre.NombrePages = updateDto.NombrePages;
        if (updateDto.Categorie != null) existingLivre.Categorie = updateDto.Categorie;
        if (updateDto.EstLu.HasValue) existingLivre.EstLu = updateDto.EstLu.Value;
        if (updateDto.Note.HasValue) existingLivre.Note = updateDto.Note;
        if (updateDto.Commentaire != null) existingLivre.Commentaire = updateDto.Commentaire;

        var updatedLivre = await _repository.UpdateAsync(id, existingLivre);
        return Ok(MapToDto(updatedLivre!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<LivreDto>>> Search([FromQuery] string query)
    {
        var livres = await _repository.SearchAsync(query);
        var livresDto = livres.Select(MapToDto);
        return Ok(livresDto);
    }

    private static LivreDto MapToDto(Livre livre)
    {
        return new LivreDto
        {
            Id = livre.Id,
            Titre = livre.Titre,
            Auteur = livre.Auteur,
            ISBN = livre.ISBN,
            DatePublication = livre.DatePublication,
            Description = livre.Description,
            ImageUrl = livre.ImageUrl,
            NombrePages = livre.NombrePages,
            Categorie = livre.Categorie,
            EstLu = livre.EstLu,
            DateAjout = livre.DateAjout,
            Note = livre.Note,
            Commentaire = livre.Commentaire
        };
    }
}