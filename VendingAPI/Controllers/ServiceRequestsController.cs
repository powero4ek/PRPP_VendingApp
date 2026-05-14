using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendingAPI.Data;
using VendingAPI.Models;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly VendingDbContext _db;
    public ServiceRequestsController(VendingDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.ServiceRequests.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) => Ok(await _db.ServiceRequests.FindAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ServiceRequest r)
    {
        r.Status = "Новая";
        r.CreatedAt = DateTime.Now;
        _db.ServiceRequests.Add(r);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = r.RequestID }, r);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ServiceRequest r)
    {
        var existing = await _db.ServiceRequests.FindAsync(id);
        if (existing == null) return NotFound();
        existing.UserID = r.UserID;
        existing.ScheduledDate = r.ScheduledDate;
        existing.Status = r.Status;
        existing.Description = r.Description;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignDto dto)
    {
        var req = await _db.ServiceRequests.FindAsync(id);
        if (req == null) return NotFound();
        req.UserID = dto.UserID;
        req.Status = "В работе";
        await _db.SaveChangesAsync();
        var machine = await _db.Machines.FindAsync(req.MachineID);
        if (machine != null) machine.MachineStatus = "В ремонте/на обслуживании";
        await _db.SaveChangesAsync();
        return Ok(req);
    }
}

public class AssignDto { public int UserID { get; set; } }
