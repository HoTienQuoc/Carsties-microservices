using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config, AppDbContext dbContext)
    {
        _httpClient = httpClient;
        _config = config;
        _dbContext = dbContext;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        // Lấy UpdatedAt mới nhất từ database
        var lastUpdated = await _dbContext.Items
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => x.UpdatedAt)
            .FirstOrDefaultAsync();

        // Nếu chưa có item nào, dùng ngày mặc định
        var startDate = lastUpdated != default ? lastUpdated : new DateTime(2023, 1, 1);
        var isoDate = startDate.ToUniversalTime().ToString("o"); // ISO 8601, UTC

        // Gọi Auction service với date hợp lệ
        var url = $"{_config["AuctionServiceUrl"]}/api/auctions?date={Uri.EscapeDataString(isoDate)}";
        var response = await _httpClient.GetAsync(url);

        // Nếu lỗi HTTP, sẽ throw để Polly retry
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<Item>>();

        return items ?? new List<Item>();
    }
}
