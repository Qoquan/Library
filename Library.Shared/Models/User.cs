// =============================================================
// Fichier : Library.Shared/Models/User.cs
// Rôle    : Modèle représentant un utilisateur de l'application.
//           Partagé entre l'API et le frontend Blazor.
// =============================================================

namespace Library.Shared.Models
{
    /// <summary>
    /// Représente un utilisateur enregistré dans le système.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO pour la requête de login / register.
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO retourné après un login ou register réussi.
    /// </summary>
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}