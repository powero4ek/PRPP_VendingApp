using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class Protocol
{
    [Key]
    public int ProtocolID { get; set; }
    public int? RequestID { get; set; }
    public int MachineID { get; set; }
    public int? UserID { get; set; }
    public string? ProtocolType { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PdfPath { get; set; }
}
