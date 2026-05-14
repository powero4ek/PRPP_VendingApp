using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;
using VendingAPI.Models;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly VendingDbContext _db;
    public MachinesController(VendingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var q = _db.Machines.AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(m => m.MachineStatus == status);
        if (!string.IsNullOrEmpty(search)) q = q.Where(m => m.Location.Contains(search) || m.Model.Contains(search));
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var m = await _db.Machines.FindAsync(id);
        return m == null ? NotFound() : Ok(m);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Machine machine)
    {
        _db.Machines.Add(machine);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = machine.MachineID }, machine);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Machine machine)
    {
        var existing = await _db.Machines.FindAsync(id);
        if (existing == null) return NotFound();
        _db.Entry(existing).CurrentValues.SetValues(machine);
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Machines.FindAsync(id);
        if (m == null) return NotFound();
        _db.Machines.Remove(m);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Удалено" });
    }

    [HttpPost("{id}/detach-modem")]
    public async Task<IActionResult> DetachModem(int id)
    {
        var m = await _db.Machines.FindAsync(id);
        if (m == null) return NotFound();
        m.ModemID = null;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Модем отвязан", modemId = -1 });
    }

    [HttpGet("monitor")]
    public async Task<IActionResult> Monitor([FromQuery] string? status, [FromQuery] string? paymentType, [FromQuery] string? connectionStatus)
    {
        var q = _db.Machines.AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(m => m.MachineStatus == status);
        if (!string.IsNullOrEmpty(paymentType)) q = q.Where(m => m.PaymentType == paymentType);
        var list = await q.ToListAsync();
        var result = list.Select(m => new {
            m.MachineID, m.Location, m.Model, m.PaymentType, m.MachineStatus, m.Country,
            ConnectionStatus = m.MachineStatus == "Работает" ? "Стабильная" : (m.MachineStatus == "В ремонте/на обслуживании" ? "Прервана" : "Отсутствует"),
            LoadPercent = m.MachineStatus == "Работает" ? new Random().Next(50, 100) : new Random().Next(0, 30),
            MoneyInMachine = new Random().Next(1000, 50000),
            m.LastVerificationDate, m.DateOfNextFixing, m.ResourceHours
        });
        return Ok(result);
    }
}
