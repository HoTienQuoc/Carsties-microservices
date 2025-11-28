using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Services;
using MassTransit;
using Contracts;
using SearchService.Consumers;





var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk0NjE0NDAwIiwiaWF0IjoiMTc2MzEyNDM5MyIsImFjY291bnRfaWQiOiIwMTlhODI2NjcyYmY3MTc5YTAxZTIyMzQ2MGY1MjE5ZiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2ExNmVkZzE5bWZlYzFzcTB2NnRuN2pyIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.BCqaDSgBujuQoTK_L6sDO_fgQXtdRrqkwfxixEZTDj2DwUoU1mAclNMYxtu6E1Gg16KwgLaybB_KjTcjPlrPjsZiZvZDL34gZbHMLOXXzKKsh30MqmEqpD0Z8gZ3bIVsSsh6hGaF1DXXWV-q1sSof8Nk32xMuvo9VqpNtM96nWBAQii4oGEjEUV2GQoYDTR-O11IuIlcTdkaU4HzMgH3PZG2BdER-Llc2kQnPIM3RadLFkYXjrd6m4D4N0v2VZUDErMHX5q8OfhhdWBA7fHm8vduRbpOKeswn-VoDS9SO6cud1RYAOmfARW6qPDO0Hg6gYYw0pDetMK4H8DMcZr_-w";
}, AppDomain.CurrentDomain.GetAssemblies());


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
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    // A Transport
    x.UsingRabbitMq((
        context,
        cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
            {
                h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
                h.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
            });
            cfg.ReceiveEndpoint("search-auction-created", e =>
            {
                e.UseMessageRetry(r => r.Interval(
                    5,
                    5));
                e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            });
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



