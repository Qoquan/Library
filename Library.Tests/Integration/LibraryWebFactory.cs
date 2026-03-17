// =============================================================
// Fichier : Library.Tests/Integration/LibraryWebFactory.cs
// Rôle    : Fabrique de serveur de test pour les tests d'intégration.
//           L'API utilise maintenant des stores en mémoire —
//           pas besoin de remplacer EF Core.
// =============================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.Tests.Integration
{
    public class LibraryWebFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Les stores (UserStore, UserBookStore) sont des Singletons en mémoire.
            // Rien à remplacer : l'API ne dépend plus de EF Core / SQLite.
            builder.UseEnvironment("Testing");
        }
    }
}