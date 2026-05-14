using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;
using VendingAPI.Models;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly VendingDbContext _db;
    public SalesController(VendingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var q = _db.Sales.AsQueryable();
        if (from.HasValue) q = q.Where(s => s.SaleDateTime >= from);
        if (to.HasValue) q = q.Where(s => s.SaleDateTime <= to);
        return Ok(await q.OrderByDescending(s => s.SaleDateTime).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await _db.Sales.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Sale s)
    {
        _db.Sales.Add(s);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = s.SaleID }, s);
    }

    [HttpGet("stats/last10days")]
    public async Task<IActionResult> Last10Days()
    {
        var from = DateTime.Now.AddDays(-10);
        var data = await _db.Sales
            .Where(s => s.SaleDateTime >= from)
            .GroupBy(s => s.SaleDateTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count(), Sum = g.Sum(s => s.SaleSum) })
            .ToListAsync();
        return Ok(data);
    }
}
