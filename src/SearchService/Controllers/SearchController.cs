using Microsoft.AspNetCore.Mvc;
using SearchService.Data;
using SearchService.Models;
using Microsoft.EntityFrameworkCore;  // <--- bắt buộc


namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{

    private readonly AppDbContext _dbContext;

    public SearchController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> SearchItems(
    [FromQuery] string? searchTerm,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 4)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 4;

        // Bắt đầu query
        IQueryable<Item> query = _dbContext.Items.AsQueryable();

        // Filter theo searchTerm nếu có
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(i =>
                i.Make.ToLower().Contains(searchTerm.ToLower()) ||
                i.Model.ToLower().Contains(searchTerm.ToLower())
            );
        }

        // Lấy tổng số bản ghi trước khi phân trang
        var totalCount = await query.CountAsync();

        // Tính tổng số trang
        var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Sắp xếp và phân trang
        var results = await query
            .OrderBy(i => i.Make)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Trả về JSON
        return Ok(new
        {
            results,
            pageCount,
            totalCount
        });
    }



}
