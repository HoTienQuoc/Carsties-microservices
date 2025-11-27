using System;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class AuctionFinishedConsumer(AppDbContext context) : IConsumer<AuctionFinished>
    {
        private readonly AppDbContext _context = context;

        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> Consuming auction finished");

            var auction = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == context.Message.AuctionId) ?? throw new MessageException(typeof(AuctionFinished), "Cannot retrieve this auction");
            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount ?? 0;
            }

            auction.Status = "Finished";

            _context.Items.Update(auction);
            await _context.SaveChangesAsync();
        }
    }
}
