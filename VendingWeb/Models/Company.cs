using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;
public class Company
{
    [Key]
    public int CompanyID { get; set; }
    public string Name { get; set; } = "";
}
