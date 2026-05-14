using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;

public class Maintenance
{
    [Key]
    public int NoteID { get; set; }
    public int MachineID { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public string? Problems { get; set; }
    public int? DoneByUser { get; set; }
    public int? ProtocolID { get; set; }
}