using BibliothequePersonnelle.Core.DTOs ;
using BibliothequePersonnelle.Core.Interfaces;
using Newtonsoft.Json;

namespace BibliothequePersonnelle.Infrastructure.Services;

public class GoogleBookService : IGoogleBookService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.googleapis.com/books/v1/volumes";

    public GoogleBookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<IEnumerable<ExternalBookDto  >> SearchBooksAsync(string query)
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}?q={Uri.EscapeDataString(query)}");
        var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

        if (result?.Items == null)
            return Enumerable.Empty<ExternalBookDto>();

        return result.Items.Select(MapToExternalBookDto);

    }
    
    public async Task<ExternalBookDto?> GetBookByISBNAsync(string isbn)
    {
        var response = await _httpClient.GetStringAsync($"{BaseUrl}?q=isbn:{Uri.EscapeDataString(isbn)}");
        var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

        if (result?.Items == null || !result.Items.Any())
            return null;

        return MapToExternalBookDto(result.Items.First());
    }

    private ExternalBookDto MapToExternalBookDto(GoogleBookItem item)
    {
        