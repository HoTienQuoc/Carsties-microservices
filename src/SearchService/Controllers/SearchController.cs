using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> SearchItems([FromQuery] SearchParams searchParams)
    {
        IQueryable<Item> query = _db.Items.AsQueryable();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            var term = searchParams.SearchTerm.ToLower();

            query = query.Where(i =>
                i.Make.ToLower().Contains(term) ||
                i.Model.ToLower().Contains(term));
        }

        query = searchParams.FilterBy?.ToLower() switch
        {
            "finished" =>
                query.Where(i => i.AuctionEnd < DateTime.UtcNow),

            "endingsoon" =>
                query.Where(i =>
                    i.AuctionEnd > DateTime.UtcNow &&
                    i.AuctionEnd < DateTime.UtcNow.AddHours(6)),

            _ =>
                query.Where(i => i.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
            query = query.Where(i => i.Seller == searchParams.Seller);

        if (!string.IsNullOrEmpty(searchParams.Winner))
            query = query.Where(i => i.Winner == searchParams.Winner);

        query = searchParams.OrderBy?.ToLower() switch
        {
            "make" =>
                query.OrderBy(i => i.Make).ThenBy(i => i.Model),

            "new" =>
                query.OrderByDescending(i => i.CreatedAt),

            _ =>
                query.OrderBy(i => i.AuctionEnd)
        };

        var totalCount = await query.CountAsync();

        var pageCount = (int)Math.Ceiling(
            totalCount / (double)searchParams.PageSize
        );

        var results = await query
            .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToListAsync();

        return Ok(new
        {
            results,
            pageCount,
            totalCount,
            pageNumber = searchParams.PageNumber,
            pageSize = searchParams.PageSize
        });
    }
}
