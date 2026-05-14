using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;
using VendingAPI.Models;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly VendingDbContext _db;
    public MaintenanceController(VendingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Maintenances.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await _db.Maintenances.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Maintenance m)
    {
        _db.Maintenances.Add(m);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = m.NoteID }, m);
    }
}
