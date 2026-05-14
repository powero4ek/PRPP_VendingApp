using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;

public class Incassation
{
    [Key]
    public int IncassationID { get; set; }
    public int MachineID { get; set; }
    public decimal Amount { get; set; }
    public DateTime IncassationDate { get; set; }
    public int? DoneByUser { get; set; }
}