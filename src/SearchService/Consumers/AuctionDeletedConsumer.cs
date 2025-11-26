using System;
using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Data;
using Microsoft.EntityFrameworkCore;


namespace SearchService.Consumers;

public class AuctionDeletedConsumer(IMapper mapper, AppDbContext context) : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _context = context;


    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"Auction deleted: {context.Message.Id}");

        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == context.Message.Id) ?? throw new Exception($"Auction {context.Message.Id} not found");
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }

}
