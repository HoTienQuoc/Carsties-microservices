using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Services;
using MassTransit;





var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Đọc connection string từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("MongoDb");

// Đăng ký MongoClient để DI có thể inject vào controller

builder.Services.AddHttpClient<AuctionSvcHttpClient>();

var databaseName = builder.Configuration["DatabaseName"];

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("MongoDB connection string or database name is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMongoDB(connectionString, databaseName);
});



builder.Services.AddHttpClient<AuctionSvcHttpClient>()
    .AddStandardResilienceHandler(options =>
    {
        // Retry configuration
        options.Retry.MaxRetryAttempts = 6;
        options.Retry.Delay = TimeSpan.FromSeconds(2); // fallback delay nếu DelayGenerator không được dùng
        options.Retry.DelayGenerator = args =>
            new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber)));


        // Circuit breaker
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 8;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

        // Attempt timeout (mỗi lần thử)
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

        // Total request timeout (bao gồm tất cả retry)
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
    });

builder.Services.AddMassTransit(x =>
{
    // A Transport
    x.UsingRabbitMq((
        context,
        cfg) =>
    {
        cfg.ConfigureEndpoints(context);

    });
});




var app = builder.Build();


// // Đọc file JSON
// string filePath = "Data/auctions.json";
// if (!File.Exists(filePath))
// {
//     throw new FileNotFoundException($"Không tìm thấy file JSON: {filePath}");
// }

// string jsonString = File.ReadAllText(filePath);
// var options = new JsonSerializerOptions
// {
//     PropertyNameCaseInsensitive = true
// };

// // Deserialize JSON thành danh sách Item
// List<Item> items = JsonSerializer.Deserialize<List<Item>>(jsonString, options)!;

// // Hiển thị dữ liệu trên console (tuỳ chọn)
// foreach (var item in items)
// {
//     Console.WriteLine($"{item.Make} {item.Model}, Seller: {item.Seller}, Status: {item.Status}");
// }

// // Insert dữ liệu vào MongoDB
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//     // Kiểm tra nếu chưa có dữ liệu, tránh duplicate
//     if (!dbContext.Items.Any())
//     {
//         dbContext.Items.AddRange(items);
//         dbContext.SaveChanges();
//         Console.WriteLine($"Đã thêm {items.Count} item vào MongoDB thành công!");
//     }
//     else
//     {
//         Console.WriteLine("Dữ liệu đã tồn tại, không insert nữa.");
//     }
// }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});


app.Run();



