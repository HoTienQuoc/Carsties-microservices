using System;
using AutoMapper;
using Contracts;
using MassTransit;
using SearchService.Data;
using Microsoft.EntityFrameworkCore;


namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper, AppDbContext context) : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _context = context;

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"Auction updated: {context.Message.Id}");

        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == context.Message.Id) ?? throw new Exception($"Auction {context.Message.Id} not found");
        _mapper.Map(context.Message, item); // map các field mới lên entity

        await _context.SaveChangesAsync();
    }

}
