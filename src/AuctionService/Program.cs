using AuctionService.Data;
using AuctionService.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using AuctionService.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk0NjE0NDAwIiwiaWF0IjoiMTc2MzEyNDM5MyIsImFjY291bnRfaWQiOiIwMTlhODI2NjcyYmY3MTc5YTAxZTIyMzQ2MGY1MjE5ZiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa2ExNmVkZzE5bWZlYzFzcTB2NnRuN2pyIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.BCqaDSgBujuQoTK_L6sDO_fgQXtdRrqkwfxixEZTDj2DwUoU1mAclNMYxtu6E1Gg16KwgLaybB_KjTcjPlrPjsZiZvZDL34gZbHMLOXXzKKsh30MqmEqpD0Z8gZ3bIVsSsh6hGaF1DXXWV-q1sSof8Nk32xMuvo9VqpNtM96nWBAQii4oGEjEUV2GQoYDTR-O11IuIlcTdkaU4HzMgH3PZG2BdER-Llc2kQnPIM3RadLFkYXjrd6m4D4N0v2VZUDErMHX5q8OfhhdWBA7fHm8vduRbpOKeswn-VoDS9SO6cud1RYAOmfARW6qPDO0Hg6gYYw0pDetMK4H8DMcZr_-w";
}, typeof(MappingProfiles).Assembly);



builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(opt =>
    {
        opt.QueryDelay = TimeSpan.FromSeconds(1);
        opt.UsePostgres();
        opt.UseBusOutbox();
    });
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
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
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while migrating or seeding the database: {ex.Message}");
    throw; // Re-throw the exception after logging it
}

app.Run();


