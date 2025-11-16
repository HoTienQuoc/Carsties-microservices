using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
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
        modelBuilder.Entity<Item>()
            .ToCollection("items");
    }

}
