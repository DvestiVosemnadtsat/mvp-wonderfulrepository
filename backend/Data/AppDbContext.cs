using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Media> Medias => Set<Media>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Media>().HasData(
            new Media
            {
                Id = 1,
                Name = "123",
                pagesQuan = 113
            },
            new Media
            {
                Id = 2,
                Name = "456",
                pagesQuan = 218
            }
        );
    }
}