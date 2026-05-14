using System.ComponentModel.DataAnnotations;
namespace VendingWeb.Models;
public class UserModel
{
    [Key]
    public int UserModelID { get; set; }
    public int UserID { get; set; }
    public string ModelName { get; set; } = "";
}
