using System.Net.Http.Json;
using BibliothequePersonnelle.Core.DTOs;

namespace BibliothequePersonnelle.Client.Services;

public class LivreService : ILivreService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "api/livres";

    public LivreService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<LivreDto>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<LivreDto>>(ApiUrl) 
            ?? new List<LivreDto>();
    }

    public async Task<LivreDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<LivreDto>($"{ApiUrl}/{id}");
    }

    public async Task<LivreDto> CreateAsync(CreateLivreDto createDto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiUrl, createDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LivreDto>() 
            ?? throw new Exception("Failed to create livre");
    }

    public async Task<LivreDto> UpdateAsync(int id, UpdateLivreDto updateDto)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/{id}", updateDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LivreDto>() 
            ?? throw new Exception("Failed to update livre");
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<LivreDto>> SearchAsync(string query)
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<LivreDto>>($"{ApiUrl}/search?query={Uri.EscapeDataString(query)}") 
            ?? new List<LivreDto>();
    }

    public Task<IEnumerable<LivreDto>> GetAllLivresAsync()
    {
        throw new NotImplementedException();
    }

    public Task<LivreDto?> GetLivreByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<LivreDto> CreateLivreAsync(CreateLivreDto createDto)
    {
        throw new NotImplementedException();
    }

    public Task<LivreDto?> UpdateLivreAsync(int id, UpdateLivreDto updateDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteLivreAsync(int id)
    {
        throw new NotImplementedException();
    }
}