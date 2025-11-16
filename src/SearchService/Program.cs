using Microsoft.EntityFrameworkCore;
using SearchService.Data;

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
    options.UseMongoDB(connectionString, databaseName));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();



app.Run();
