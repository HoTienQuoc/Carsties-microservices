using Microsoft.EntityFrameworkCore;
using SearchService.Models;

namespace SearchService.Data;

public class AppDbContext : DbContext
{

    public DbSet<Item> Items { get; set; }

    public AppDbContext(DbContextOptions options)
        : base(options)
    {
        this.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tạo text index trên Make, Model, Color
        modelBuilder.Entity<Item>()
            .HasIndex(i => new { i.Make, i.Model, i.Color });
    }


}
