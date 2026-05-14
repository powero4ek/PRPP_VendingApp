using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class User
{
    [Key]
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Contacts { get; set; }
    public string Role { get; set; } = "Оператор";
    public string PasswordHash { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string? TabNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
