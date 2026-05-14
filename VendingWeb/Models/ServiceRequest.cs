using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;
public class ServiceRequest
{
    [Key]
    public int RequestID { get; set; }
    public int MachineID { get; set; }
    public Machine? Machine { get; set; }
    public int? UserID { get; set; }
    public User? User { get; set; }
    public string RequestType { get; set; } = "";
    public string Status { get; set; } = "Новая";
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Description { get; set; }
}
