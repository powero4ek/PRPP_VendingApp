using System.ComponentModel.DataAnnotations;
namespace VendingAPI.Models;
public class Product
{
    [Key]
    public int ProductID { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int InStock { get; set; }
    public int MinStock { get; set; }
    public decimal? PropensityToSell { get; set; }
}
