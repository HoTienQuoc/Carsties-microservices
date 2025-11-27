using System;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Consumers
{
    public class BidPlacedConsumer(AppDbContext context) : IConsumer<BidPlaced>
    {
        private readonly AppDbContext _context = context;

        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--> Consuming bid placed");

            var auction = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == context.Message.AuctionId) ?? throw new MessageException(typeof(BidPlaced), "Cannot retrieve this auction");

            // Kiểm tra bid cao nhất
            if (auction.CurrentHighBid == null ||
                (context.Message.BidStatus.Contains("Accepted") &&
                 context.Message.Amount > auction.CurrentHighBid))
            {
                auction.CurrentHighBid = context.Message.Amount;
                _context.Items.Update(auction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
