using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SearchService.Data;
using SearchService.Models;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{

    private readonly IMongoCollection<Item> _itemsCollection;

    public SearchController(IMongoClient client, IConfiguration config)
    {
        var dbName = config["DatabaseName"];
        var database = client.GetDatabase(dbName);
        _itemsCollection = database.GetCollection<Item>("items");

    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> GetAllAsync()
    {
        var items = await _itemsCollection.Find(_ => true).ToListAsync(); // async
        return Ok(items);
    }
}
