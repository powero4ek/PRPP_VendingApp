using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;

public class Machine
{
    [Key]
    public int MachineID { get; set; }
    public string Location { get; set; } = "";
    public string Model { get; set; } = "";
    public string PaymentType { get; set; } = "";
    public decimal FullIncome { get; set; }
    public string SerialNumber { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string? Manufacturer { get; set; }
    public DateTime ManufactureDate { get; set; }
    public DateTime DateOfCommissioning { get; set; }
    public DateTime? LastVerificationDate { get; set; }
    public int? VerificationInterval { get; set; }
    public int? ResourceHours { get; set; }
    public DateTime? DateOfNextFixing { get; set; }
    public int? MaintenanceTimeHours { get; set; }
    public string MachineStatus { get; set; } = "Работает";
    public string? Country { get; set; }
    public DateTime? InventoryDate { get; set; }
    public DateTime DateAdded { get; set; }
    public int? LastCheckedByUser { get; set; }
    public int? CompanyID { get; set; }
    public Company? Company { get; set; }
    public int? ModemID { get; set; }
}