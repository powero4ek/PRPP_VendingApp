using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class StatusHistory
{
    [Key]
    public int HistoryID { get; set; }
    public string EntityType { get; set; } = "";
    public int EntityID { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = "";
    public DateTime ChangedAt { get; set; }
    public int? ChangedBy { get; set; }
}
