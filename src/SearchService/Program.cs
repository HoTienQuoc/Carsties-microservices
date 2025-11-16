using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using SearchService.Data;
using SearchService.Models;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();


var connectionString = builder.Configuration.GetConnectionString("MongoDb");
var databaseName = builder.Configuration["DatabaseName"];

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("MongoDB connection string or database name is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMongoDB(connectionString, databaseName);
});




var app = builder.Build();


// Đọc file JSON
string filePath = "Data/auctions.json";
if (!File.Exists(filePath))
{
    throw new FileNotFoundException($"Không tìm thấy file JSON: {filePath}");
}

string jsonString = File.ReadAllText(filePath);
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

// Deserialize JSON thành danh sách Item
List<Item> items = JsonSerializer.Deserialize<List<Item>>(jsonString, options)!;

// Hiển thị dữ liệu trên console (tuỳ chọn)
foreach (var item in items)
{
    Console.WriteLine($"{item.Make} {item.Model}, Seller: {item.Seller}, Status: {item.Status}");
}

// Insert dữ liệu vào MongoDB
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Kiểm tra nếu chưa có dữ liệu, tránh duplicate
    if (!dbContext.Items.Any())
    {
        dbContext.Items.AddRange(items);
        dbContext.SaveChanges();
        Console.WriteLine($"Đã thêm {items.Count} item vào MongoDB thành công!");
    }
    else
    {
        Console.WriteLine("Dữ liệu đã tồn tại, không insert nữa.");
    }
}









// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();



app.Run();
