using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;
using VendingAPI.Models;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly VendingDbContext _db;
    public ProductsController(VendingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Products.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await _db.Products.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product p)
    {
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.ProductID }, p);
    }
}
