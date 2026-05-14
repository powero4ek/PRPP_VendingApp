using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VendingDesktop.Models;
using VendingDesktop.Services;

namespace VendingDesktop.ViewModels;

public class AdminViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly NotificationService _notif;
    private ObservableCollection<Machine> _machines = new();
    private string _searchText = "";
    private int _page = 1;
    private int _pageSize = 10;
    private int _total;
    private Machine? _selectedMachine;

    public ObservableCollection<Machine> Machines { get => _machines; set { _machines = value; OnPropertyChanged(); } }
    public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
    public int Page { get => _page; set { _page = value; OnPropertyChanged(); } }
    public int PageSize { get => _pageSize; set { _pageSize = value; OnPropertyChanged(); } }
    public int Total { get => _total; set { _total = value; OnPropertyChanged(); } }
    public Machine? SelectedMachine { get => _selectedMachine; set { _selectedMachine = value; OnPropertyChanged(); } }
    public string PageInfo => $"{Machines.Count} из {Total}";

    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand DetachModemCommand { get; }
    public ICommand AddCommand { get; }

    public AdminViewModel(ApiService api, NotificationService notif)
    {
        _api = api; _notif = notif;
        SearchCommand = new RelayCommand(async _ => await LoadAsync());
        NextPageCommand = new RelayCommand(async _ => { Page++; await LoadAsync(); }, _ => Page * PageSize < Total);
        PrevPageCommand = new RelayCommand(async _ => { Page--; await LoadAsync(); }, _ => Page > 1);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedMachine != null);
        DetachModemCommand = new RelayCommand(async _ => await DetachAsync(), _ => SelectedMachine != null && SelectedMachine.ModemID > 0);
        AddCommand = new RelayCommand(async _ => await AddAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var data = await _api.GetMachinesAsync(null, SearchText, Page, PageSize);
        if (data != null)
        {
            Machines = new ObservableCollection<Machine>(data);
            Total = data.Count > 0 ? data.Count * 2 : 0; // упрощенно
            OnPropertyChanged(nameof(PageInfo));
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedMachine == null) return;
        if (MessageBox.Show($"Удалить ТА #{SelectedMachine.MachineID}?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DeleteMachineAsync(SelectedMachine.MachineID))
        {
            _notif.Show("ТА удален", NotificationType.Info, 5000);
            await LoadAsync();
        }
    }

    private async Task DetachAsync()
    {
        if (SelectedMachine == null) return;
        if (MessageBox.Show("Отвязать модем?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        if (await _api.DetachModemAsync(SelectedMachine.MachineID))
        {
            SelectedMachine.ModemID = -1;
            _notif.Show("Модем отвязан", NotificationType.Info, 5000);
            OnPropertyChanged(nameof(Machines));
        }
    }

    private async Task AddAsync()
    {
        var m = new Machine
        {
            Location = "Новое место",
            Model = "VendCore X-200",
            PaymentType = "с оплатой картой",
            SerialNumber = $"SN{DateTime.Now:yyyyMMddHHmmss}",
            InventoryNumber = $"INV-{DateTime.Now:yyyy}-{new Random().Next(100,999)}",
            ManufactureDate = DateTime.Now.AddYears(-1),
            DateOfCommissioning = DateTime.Now.AddMonths(-6),
            MachineStatus = "Работает",
            Country = "Россия",
            DateAdded = DateTime.Now
        };
        if (await _api.CreateMachineAsync(m))
        {
            _notif.Show("ТА добавлен", NotificationType.Info, 5000);
            await LoadAsync();
        }
    }
}
