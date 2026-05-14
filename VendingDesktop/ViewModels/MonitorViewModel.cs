using System.Collections.ObjectModel;
using System.Windows.Input;
using VendingDesktop.Models;
using VendingDesktop.Services;

namespace VendingDesktop.ViewModels;

public class MonitorViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly NotificationService _notif;
    private ObservableCollection<Machine> _machines = new();
    private string? _filterStatus;
    private string? _filterPayment;
    private int _totalMachines;
    private decimal _totalMoney;

    public ObservableCollection<Machine> Machines { get => _machines; set { _machines = value; OnPropertyChanged(); } }
    public string? FilterStatus { get => _filterStatus; set { _filterStatus = value; OnPropertyChanged(); } }
    public string? FilterPayment { get => _filterPayment; set { _filterPayment = value; OnPropertyChanged(); } }
    public int TotalMachines { get => _totalMachines; set { _totalMachines = value; OnPropertyChanged(); } }
    public decimal TotalMoney { get => _totalMoney; set { _totalMoney = value; OnPropertyChanged(); } }

    public ICommand ApplyFilterCommand { get; }
    public ICommand SortByStatusCommand { get; }

    public MonitorViewModel(ApiService api, NotificationService notif)
    {
        _api = api; _notif = notif;
        ApplyFilterCommand = new RelayCommand(async _ => await LoadAsync());
        SortByStatusCommand = new RelayCommand(_ => SortByStatus());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var data = await _api.GetMonitorDataAsync(FilterStatus, FilterPayment);
        if (data != null)
        {
            Machines = new ObservableCollection<Machine>(data);
            TotalMachines = data.Count;
            TotalMoney = data.Sum(m => m.MoneyInMachine);
        }
        else
        {
            _notif.Show("Нет данных для отображения", NotificationType.Warning, 5000);
        }
    }

    private void SortByStatus()
    {
        var sorted = Machines.OrderBy(m => m.MachineStatus).ToList();
        Machines = new ObservableCollection<Machine>(sorted);
    }
}
