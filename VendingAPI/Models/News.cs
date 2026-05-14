using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class News
{
    [Key]
    public int NewsID { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime PublishDate { get; set; }
    public int? CompanyID { get; set; }
}
