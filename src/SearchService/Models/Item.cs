using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace SearchService.Models;

public class Item
{
    [BsonId]
    public Guid Id { get; set; }
    public int ReservePrice { get; set; }
    public required string Seller { get; set; }
    public string? Winner { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime AuctionEnd { get; set; }
    public required String Status { get; set; }
    public required String Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
    public required string ImageUrl { get; set; }

    public bool HasReservePrice() => ReservePrice > 0;
}
