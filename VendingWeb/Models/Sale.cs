using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;

public class Sale
{
    [Key]
    public int SaleID { get; set; }
    public int MachineID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal SaleSum { get; set; }
    public DateTime SaleDateTime { get; set; }
    public string? PaymentType { get; set; }
}