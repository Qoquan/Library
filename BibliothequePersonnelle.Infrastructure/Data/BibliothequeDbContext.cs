using BibliothequePersonnelle.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BibliothequePersonnelle.Infrastructure.Data;

public class BibliothequeDbContext : DbContext
{
    public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Livre> Livres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Livre>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Auteur).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ISBN).HasMaxLength(13);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Categorie).HasMaxLength(100);
            entity.Property(e => e.Commentaire).HasMaxLength(1000);
        });
    }
}