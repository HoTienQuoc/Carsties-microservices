using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{

    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems()
    {
        var items = await _context.Items.AsQueryable().ToListAsync(); // <-- bắt buộc AsQueryable()
        Console.WriteLine(items.Count);
        return Ok(items);
    }
}
