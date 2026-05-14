using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class Company
{
    [Key]
    public int CompanyID { get; set; }
    public string Name { get; set; } = "";
    public string? INN { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
