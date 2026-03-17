// =============================================================
// Fichier : Library.Web/Services/AuthService.cs
// Rôle    : Gère l'état de connexion de l'utilisateur dans Blazor.
//           Communique avec /api/auth/login et /api/auth/register.
//           Notifie les composants quand l'utilisateur se connecte
//           ou se déconnecte (événement OnAuthChanged).
// POO     : Encapsulation, événements, injection de dépendances.
// =============================================================

using System.Net.Http.Json;
using Library.Shared.Models;

namespace Library.Web.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        // Utilisateur actuellement connecté (null si déconnecté)
        public User? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        // Événement déclenché quand l'état change → les composants
        // s'y abonnent pour se re-rendre automatiquement
        public event Action? OnAuthChanged;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<AuthResponse> RegisterAsync(string username, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register",
                    new LoginRequest { Username = username, Password = password });

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>()
                    ?? new AuthResponse { Success = false, Message = "Erreur inconnue." };

                if (result.Success && result.User != null)
                {
                    CurrentUser = result.User;
                    OnAuthChanged?.Invoke();
                }

                return result;
            }
            catch
            {
                return new AuthResponse { Success = false, Message = "Impossible de joindre l'API." };
            }
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login",
                    new LoginRequest { Username = username, Password = password });

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>()
                    ?? new AuthResponse { Success = false, Message = "Erreur inconnue." };

                if (result.Success && result.User != null)
                {
                    CurrentUser = result.User;
                    OnAuthChanged?.Invoke();
                }

                return result;
            }
            catch
            {
                return new AuthResponse { Success = false, Message = "Impossible de joindre l'API." };
            }
        }

        public void Logout()
        {
            CurrentUser = null;
            OnAuthChanged?.Invoke();
        }
    }
}