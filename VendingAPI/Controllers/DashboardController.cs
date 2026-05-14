using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly VendingDbContext _db;
    public DashboardController(VendingDbContext db) => _db = db;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var total = await _db.Machines.CountAsync();
        var working = await _db.Machines.CountAsync(m => m.MachineStatus == "Работает");
        var broken = await _db.Machines.CountAsync(m => m.MachineStatus == "Вышел из строя");
        var maintenance = await _db.Machines.CountAsync(m => m.MachineStatus == "В ремонте/на обслуживании");
        var salesSum = await _db.Sales.SumAsync(s => s.SaleSum);
        var incSum = await _db.Incassations.SumAsync(i => i.Amount);
        var maintCount = await _db.Maintenances.CountAsync();
        return Ok(new { total, working, broken, maintenance, salesSum, incSum, maintCount, efficiency = total > 0 ? (working * 100.0 / total) : 0 });
    }

    [HttpGet("news")]
    public async Task<IActionResult> News() => Ok(await _db.News.OrderByDescending(n => n.PublishDate).Take(5).ToListAsync());
}
