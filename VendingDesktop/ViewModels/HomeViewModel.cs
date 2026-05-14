using System.Collections.ObjectModel;
using System.Windows.Threading;
using VendingDesktop.Models;
using VendingDesktop.Services;

namespace VendingDesktop.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private DashboardSummary? _summary;
    private ObservableCollection<News> _news = new();
    private double _efficiency;

    public DashboardSummary? Summary { get => _summary; set { _summary = value; OnPropertyChanged(); } }
    public ObservableCollection<News> NewsList { get => _news; set { _news = value; OnPropertyChanged(); } }
    public double Efficiency { get => _efficiency; set { _efficiency = value; OnPropertyChanged(); } }

    public HomeViewModel(ApiService api)
    {
        _api = api;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Summary = await _api.GetDashboardAsync();
        if (Summary != null)
        {
            Efficiency = Summary.Efficiency;
        }
        var news = await _api.GetNewsAsync();
        if (news != null) NewsList = new ObservableCollection<News>(news);
    }
}