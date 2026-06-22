using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Media> Medias => Set<Media>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "John Doe",
                Email = "mark@gmail.com"
            },
            new User
            {
                Id = 2,
                Name = "Jane Smith",
                Email = "mark2@gmail.com"
            }); 



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