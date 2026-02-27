// =============================================================
// Fichier : Library.API/Data/LibraryDbContext.cs
// Rôle    : Contexte Entity Framework Core.
//           Représente la connexion à la base de données SQLite.
//           Contient toutes les tables (DbSet) de l'application.
// =============================================================

using Microsoft.EntityFrameworkCore;
using Library.Shared.Models;

namespace Library.API.Data
{
    /// <summary>
    /// Contexte de base de données de l'application Library.
    /// Hérite de DbContext (POO : héritage) pour bénéficier
    /// de toutes les fonctionnalités d'Entity Framework Core.
    /// </summary>
    public class LibraryDbContext : DbContext
    {
        // -------------------------------------------------------
        // Constructeur : injection des options de configuration
        // -------------------------------------------------------
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        // -------------------------------------------------------
        // Tables de la base de données
        // -------------------------------------------------------

        /// <summary>Table des livres.</summary>
        public DbSet<Book> Books => Set<Book>();

        // -------------------------------------------------------
        // Configuration du modèle (colonnes, index, etc.)
        // -------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la table Books
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
                entity.Property(b => b.Author).IsRequired().HasMaxLength(150);
                entity.Property(b => b.ISBN).HasMaxLength(20);
                entity.Property(b => b.Source).HasDefaultValue("local");

                // Index pour accélérer les recherches par titre/auteur
                entity.HasIndex(b => b.Title);
                entity.HasIndex(b => b.Author);
            });

            // Données initiales (seed) pour les tests
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Le Petit Prince",
                    Author = "Antoine de Saint-Exupéry",
                    ISBN = "978-2-07-040850-4",
                    PublishedYear = 1943,
                    Genre = "Conte philosophique",
                    Description = "Un pilote rencontre un mystérieux petit garçon dans le désert.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    Source = "local"
                },
                new Book
                {
                    Id = 2,
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "978-2-07-036822-8",
                    PublishedYear = 1949,
                    Genre = "Dystopie",
                    Description = "Un roman sur la surveillance totalitaire.",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2024, 1, 1),
                    Source = "local"
                },
                new Book
                {
                    Id = 3,
                    Title = "Harry Potter à l'école des sorciers",
                    Author = "J.K. Rowling",
                    ISBN = "978-2-07-054090-1",
                    PublishedYear = 1997,
                    Genre = "Fantasy",
                    Description = "Un jeune garçon découvre qu'il est un sorcier.",
                    IsAvailable = false,
                    CreatedAt = new DateTime(2024, 1, 1),
                    Source = "local"
                }
            );
        }
    }
}