// =============================================================
// Fichier : Library.API/Services/UserStore.cs
// Rôle    : Stockage en mémoire des utilisateurs et de leurs
//           mots de passe. Singleton partagé entre toutes les
//           requêtes. Les données disparaissent au redémarrage.
// POO     : Encapsulation, responsabilité unique.
// =============================================================

using Library.Shared.Models;

namespace Library.API.Services
{
    /// <summary>
    /// Interface du store utilisateurs.
    /// </summary>
    public interface IUserStore
    {
        AuthResponse Register(LoginRequest request);
        AuthResponse Login(LoginRequest request);
        User? GetById(int id);
    }

    /// <summary>
    /// Implémentation en mémoire du store utilisateurs.
    /// Enregistré comme Singleton : une seule instance pour
    /// toute la durée de vie de l'application.
    /// </summary>
    public class UserStore : IUserStore
    {
        // Dictionnaire : username (minuscules) → (User, motDePasse)
        private readonly Dictionary<string, (User User, string Password)> _users = new();
        private int _nextId = 1;

        // Verrou pour la sécurité des accès concurrents
        private readonly object _lock = new();

        public AuthResponse Register(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return new AuthResponse { Success = false, Message = "Nom d'utilisateur et mot de passe requis." };

            if (request.Username.Length < 3)
                return new AuthResponse { Success = false, Message = "Le nom d'utilisateur doit contenir au moins 3 caractères." };

            if (request.Password.Length < 4)
                return new AuthResponse { Success = false, Message = "Le mot de passe doit contenir au moins 4 caractères." };

            var key = request.Username.ToLower().Trim();

            lock (_lock)
            {
                if (_users.ContainsKey(key))
                    return new AuthResponse { Success = false, Message = "Ce nom d'utilisateur est déjà pris." };

                var user = new User
                {
                    Id = _nextId++,
                    Username = request.Username.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _users[key] = (user, request.Password);

                return new AuthResponse { Success = true, Message = "Compte créé avec succès.", User = user };
            }
        }

        public AuthResponse Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return new AuthResponse { Success = false, Message = "Nom d'utilisateur et mot de passe requis." };

            var key = request.Username.ToLower().Trim();

            lock (_lock)
            {
                if (!_users.TryGetValue(key, out var entry))
                    return new AuthResponse { Success = false, Message = "Nom d'utilisateur ou mot de passe incorrect." };

                if (entry.Password != request.Password)
                    return new AuthResponse { Success = false, Message = "Nom d'utilisateur ou mot de passe incorrect." };

                return new AuthResponse { Success = true, Message = "Connexion réussie.", User = entry.User };
            }
        }

        public User? GetById(int id)
        {
            lock (_lock)
            {
                return _users.Values
                    .Select(e => e.User)
                    .FirstOrDefault(u => u.Id == id);
            }
        }
    }
}