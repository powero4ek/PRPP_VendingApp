using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class Modem
{
    [Key]
    public int ModemID { get; set; }
    public string IMEI { get; set; } = "";
    public string? Model { get; set; }
    public string? Provider { get; set; }
    public string? Status { get; set; }
}
