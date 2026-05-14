namespace VendingDesktop.Models;
public class DashboardSummary
{
    public int Total { get; set; }
    public int Working { get; set; }
    public int Broken { get; set; }
    public int Maintenance { get; set; }
    public decimal SalesSum { get; set; }
    public decimal IncSum { get; set; }
    public int MaintCount { get; set; }
    public double Efficiency { get; set; }
}
