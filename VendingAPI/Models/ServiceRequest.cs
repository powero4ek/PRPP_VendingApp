using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class ServiceRequest
{
    [Key]
    public int RequestID { get; set; }
    public int MachineID { get; set; }
    public int? UserID { get; set; }
    public string RequestType { get; set; } = "";
    public string Status { get; set; } = "Новая";
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Description { get; set; }
}
