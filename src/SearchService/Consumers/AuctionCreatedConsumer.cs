using System;
using MassTransit;
using Contracts;
using AutoMapper;
using SearchService.Models;
using SearchService.Data;
namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;


    public AuctionCreatedConsumer(IMapper mapper, AppDbContext context)
    {
        this._mapper = mapper;
        this._context = context;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("--> Consuming auction created: " + context.Message.Id);
        var item = _mapper.Map<Item>(context.Message);
        _context.Items.Add(item);          // Thêm vào DbSet
        await _context.SaveChangesAsync(); // Lưu xuống MongoDB
    }
}
