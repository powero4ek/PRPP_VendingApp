using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;
public class User
{
    [Key]
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}
