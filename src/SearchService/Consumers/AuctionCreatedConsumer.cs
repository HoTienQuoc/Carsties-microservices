using System;
using MassTransit;
using Contracts;
using AutoMapper;
using SearchService.Models;
using SearchService.Data;
namespace SearchService.Consumers;

public class AuctionCreatedConsumer(IMapper mapper, AppDbContext context) : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _context = context;

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("--> Consuming auction created: " + context.Message.Id);
        var item = _mapper.Map<Item>(context.Message);
        _context.Items.Add(item);          // Thêm vào DbSet
        if (item.Model == "Foo")
        {
            throw new ArgumentNullException("Cannot sell cars with name of foo");
        }
        await _context.SaveChangesAsync(); // Lưu xuống MongoDB
    }
}
