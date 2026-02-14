using BibliothequePersonnelle.Core.DTOs;
using BibliothequePersonnelle.Core.Interfaces;
using Newtonsoft.Json;

namespace BibliothequePersonnelle.Infrastructure.Services;

public class GoogleBooksService : IExternalBookService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.googleapis.com/books/v1/volumes";

    public GoogleBooksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ExternalBookDto>> SearchBooksAsync(string query)
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}?q={Uri.EscapeDataString(query)}&maxResults=10");
        var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

        if (result?.Items == null)
            return Enumerable.Empty<ExternalBookDto>();

        return result.Items.Select(MapToExternalBookDto);
    }

    public async Task<ExternalBookDto?> GetBookByISBNAsync(string isbn)
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}?q=isbn:{isbn}");
        var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

        if (result?.Items == null || !result.Items.Any())
            return null;

        return MapToExternalBookDto(result.Items.First());
    }

    private ExternalBookDto MapToExternalBookDto(GoogleBookItem item)
    {
        var volumeInfo = item.VolumeInfo;
        return new ExternalBookDto
        {
            Titre = volumeInfo.Title ?? "",
            Auteur = volumeInfo.Authors != null ? string.Join(", ", volumeInfo.Authors) : "",
            ISBN = volumeInfo.IndustryIdentifiers?.FirstOrDefault()?.Identifier,
            DatePublication = DateTime.TryParse(volumeInfo.PublishedDate, out var date) ? date : null,
            Description = volumeInfo.Description,
            ImageUrl = volumeInfo.ImageLinks?.Thumbnail?.Replace("http://", "https://"),
            NombrePages = volumeInfo.PageCount,
            Categorie = volumeInfo.Categories?.FirstOrDefault()
        };
    }

    // Classes pour la désérialisation JSON
    private class GoogleBooksResponse
    {
        public List<GoogleBookItem>? Items { get; set; }
    }

    private class GoogleBookItem
    {
        public VolumeInfo VolumeInfo { get; set; } = new();
    }

    private class VolumeInfo
    {
        public string? Title { get; set; }
        public List<string>? Authors { get; set; }
        public string? PublishedDate { get; set; }
        public string? Description { get; set; }
        public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }
        public int? PageCount { get; set; }
        public List<string>? Categories { get; set; }
        public ImageLinks? ImageLinks { get; set; }
    }

    private class IndustryIdentifier
    {
        public string? Type { get; set; }
        public string? Identifier { get; set; }
    }

    private class ImageLinks
    {
        public string? Thumbnail { get; set; }
    }
}