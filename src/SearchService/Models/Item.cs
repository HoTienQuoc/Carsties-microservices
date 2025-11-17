using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SearchService.Models;

public class Item
{
    [BsonId]
    public string Id { get; set; } = null!;

    public int ReservePrice { get; set; }
    public required string Seller { get; set; }
    public string? Winner { get; set; }

    public int? SoldAmount { get; set; }        // <- sửa thành nullable
    public int? CurrentHighBid { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }

    public required string Status { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int? Year { get; set; }              // <- cũng nên nullable nếu API trả null
    public required string Color { get; set; }
    public int? Mileage { get; set; }           // <- nullable

    public required string ImageUrl { get; set; }
}
