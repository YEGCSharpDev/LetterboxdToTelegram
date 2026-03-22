using LetterboxdToCinephilesChannel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LetterboxdToCinephilesChannel.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProcessedMovie> ProcessedMovies => Set<ProcessedMovie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProcessedMovie>()
            .HasIndex(m => m.LetterboxdId)
            .IsUnique();
    }
}
