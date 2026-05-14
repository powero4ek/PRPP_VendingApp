using Microsoft.EntityFrameworkCore;
using VendingWeb.Data;
using VendingWeb.Models;

namespace VendingWeb.Services;

public class ScheduleService
{
    private readonly VendingDbContext _db;
    public ScheduleService(VendingDbContext db) => _db = db;

    public async Task<List<MaintenanceEvent>> GetMaintenanceCalendarAsync(int? machineId = null)
    {
        var events = new List<MaintenanceEvent>();
        var machines = await _db.Machines
            .Where(m => machineId == null || m.MachineID == machineId)
            .Include(m => m.Company)
            .ToListAsync();

        foreach (var m in machines)
        {
            if (m.LastVerificationDate.HasValue && m.VerificationInterval.HasValue)
            {
                var nextDate = m.LastVerificationDate.Value.AddMonths(m.VerificationInterval.Value);
                var daysLeft = (nextDate - DateTime.Now).TotalDays;
                var color = daysLeft < 0 ? "red" : (daysLeft < 5 ? "yellow" : "green");
                events.Add(new MaintenanceEvent
                {
                    MachineID = m.MachineID,
                    Model = m.Model,
                    Location = m.Location,
                    CompanyName = m.Company?.Name,
                    Date = nextDate,
                    Color = color,
                    Type = "Плановое ТО"
                });
            }
        }
        return events;
    }

    public async Task<AssignmentResult> AssignRequestAsync(int requestId, int userId)
    {
        var req = await _db.ServiceRequests.FindAsync(requestId);
        if (req == null) return new AssignmentResult { Success = false, Message = "Заявка не найдена" };

        var machine = await _db.Machines.FindAsync(req.MachineID);
        if (machine == null) return new AssignmentResult { Success = false, Message = "ТА не найден" };

        var userModels = await _db.UserModels.Where(um => um.UserID == userId).Select(um => um.ModelName).ToListAsync();
        if (!userModels.Contains(machine.Model))
            return new AssignmentResult { Success = false, Message = "Сотрудник не обслуживает данную модель" };

        var weekStart = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(7);
        var weekTasks = await _db.ServiceRequests.CountAsync(r => r.UserID == userId && r.ScheduledDate >= weekStart && r.ScheduledDate < weekEnd);
        if (weekTasks >= 15)
            return new AssignmentResult { Success = false, Message = "Перегрузка: более 15 задач в неделю" };

        var dayTasks = await _db.ServiceRequests.CountAsync(r => r.UserID == userId && r.ScheduledDate == req.ScheduledDate);
        if (dayTasks >= 4)
            return new AssignmentResult { Success = false, Message = "Перегрузка: более 4 задач в день" };

        req.UserID = userId;
        req.Status = "В работе";
        machine.MachineStatus = "В ремонте/на обслуживании";

        _db.StatusHistories.Add(new StatusHistory
        {
            EntityType = "Request",
            EntityID = req.RequestID,
            OldStatus = "Новая",
            NewStatus = "В работе",
            ChangedAt = DateTime.Now
        });
        _db.StatusHistories.Add(new StatusHistory
        {
            EntityType = "Machine",
            EntityID = machine.MachineID,
            OldStatus = machine.MachineStatus,
            NewStatus = "В ремонте/на обслуживании",
            ChangedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
        return new AssignmentResult { Success = true };
    }

    public async Task HandleEmergencyAsync(int requestId)
    {
        var req = await _db.ServiceRequests.FindAsync(requestId);
        if (req == null || req.RequestType != "Авария") return;
        req.ScheduledDate = DateTime.Now.Date;
        req.Priority = 999;
        await _db.SaveChangesAsync();
    }
}

public class MaintenanceEvent
{
    public int MachineID { get; set; }
    public string? Model { get; set; }
    public string? Location { get; set; }
    public string? CompanyName { get; set; }
    public DateTime Date { get; set; }
    public string Color { get; set; } = "green";
    public string Type { get; set; } = "";
}

public class AssignmentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}
