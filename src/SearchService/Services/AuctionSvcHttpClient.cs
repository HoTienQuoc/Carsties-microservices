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

        // Nếu chưa có item nào, gửi request từ đầu
        var dateQuery = lastUpdated != default ? lastUpdated.ToString("o") : string.Empty;

        var items = await _httpClient.GetFromJsonAsync<List<Item>>(
            $"{_config["AuctionServiceUrl"]}/api/auctions?date={dateQuery}"
        );

        return items ?? new List<Item>();
    }
}
