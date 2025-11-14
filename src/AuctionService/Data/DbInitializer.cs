using System;
using System.Collections.Generic;
using System.Linq;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        SeedData(scope.ServiceProvider.GetService<AuctionDbContext>());
    }

    private static void SeedData(AuctionDbContext context)
    {
        // Apply any pending migrations
        context.Database.Migrate();

        if (context.Auctions.Any())
        {
            Console.WriteLine("Already have data - no seed to seed");
            return; // DB has been seeded
        }

        var auctions = new List<Auction>
        {
            new Auction
            {
                Id = Guid.NewGuid(),
                ReservePrice = 5000,
                Seller = "John Doe",
                Winner = "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuctionEnd = DateTime.UtcNow.AddDays(7),
                Status = Status.Live,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "Ford",
                    Model = "Mustang",
                    Year = 1965,
                    Color = "Red",
                    Mileage = 120000,
                    ImageUrl = "https://example.com/images/mustang.jpg"
                }
            },
            new Auction
            {
                Id = Guid.NewGuid(),
                ReservePrice = 2000,
                Seller = "Alice Smith",
                Winner = "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuctionEnd = DateTime.UtcNow.AddDays(5),
                Status = Status.Live,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "Van Gogh",
                    Model = "Starry Night",
                    Year = 1889,
                    Color = "Multicolor",
                    Mileage = 0,
                    ImageUrl = "https://example.com/images/starry-night.jpg"
                }
            },
            new Auction
            {
                Id = Guid.NewGuid(),
                ReservePrice = 300,
                Seller = "Bob Johnson",
                Winner = "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuctionEnd = DateTime.UtcNow.AddDays(3),
                Status = Status.Live,
                Item = new Item
                {
                    Id = Guid.NewGuid(),
                    Make = "Apple",
                    Model = "iPhone 15",
                    Year = 2025,
                    Color = "Black",
                    Mileage = 0,
                    ImageUrl = "https://example.com/images/iphone15.jpg"
                }
            }
        };

        // Gắn AuctionId cho Item
        foreach (var auction in auctions)
        {
            auction.Item.AuctionId = auction.Id;
            auction.Item.Auction = auction;
        }

        context.AddRange(auctions);
        context.SaveChanges();

        Console.WriteLine("Seed data successfully added.");
    }
}
