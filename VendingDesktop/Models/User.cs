namespace VendingDesktop.Models;
public class User
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string? TabNumber { get; set; }
}
