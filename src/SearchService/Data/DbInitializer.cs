using Microsoft.EntityFrameworkCore;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        // Lấy DbContext từ DI
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Kiểm tra số lượng items hiện tại
        var count = await dbContext.Items.CountAsync();
        Console.WriteLine($"Current items in DB: {count}");

        // Lấy service HTTP client để gọi Auction service
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();

        Console.WriteLine($"Items fetched from Auction service: {items.Count}");

        if (items.Count > 0)
        {
            // Upsert: nếu muốn update item trùng Id, dùng AddOrUpdate logic
            foreach (var item in items)
            {
                var existing = await dbContext.Items
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (existing == null)
                    await dbContext.Items.AddAsync(item);
                else
                    dbContext.Entry(existing).CurrentValues.SetValues(item);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
