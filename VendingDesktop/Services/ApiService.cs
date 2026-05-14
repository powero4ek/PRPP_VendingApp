using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using VendingDesktop.Models;

namespace VendingDesktop.Services;

public class ApiService
{
    private readonly HttpClient _client;
    public string? Token { get; set; }

    public ApiService(HttpClient client) => _client = client;

    private void AddAuth()
    {
        _client.DefaultRequestHeaders.Authorization = 
            string.IsNullOrEmpty(Token) ? null : new AuthenticationHeaderValue("Bearer", Token);
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var json = JsonConvert.SerializeObject(new { email, password });
        var res = await _client.PostAsync("auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode) return null;
        return JsonConvert.DeserializeObject<AuthResponse>(await res.Content.ReadAsStringAsync());
    }

    public async Task<List<Machine>?> GetMachinesAsync(string? status = null, string? search = null, int page = 1, int pageSize = 10)
    {
        AddAuth();
        var url = $"machines?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={search}";
        var res = await _client.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        var data = JsonConvert.DeserializeObject<PagedResult<Machine>>(await res.Content.ReadAsStringAsync());
        return data?.Items;
    }

    public async Task<Machine?> GetMachineAsync(int id)
    {
        AddAuth();
        var res = await _client.GetAsync($"machines/{id}");
        if (!res.IsSuccessStatusCode) return null;
        return JsonConvert.DeserializeObject<Machine>(await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> DeleteMachineAsync(int id)
    {
        AddAuth();
        var res = await _client.DeleteAsync($"machines/{id}");
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DetachModemAsync(int id)
    {
        AddAuth();
        var res = await _client.PostAsync($"machines/{id}/detach-modem", null);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<Machine>?> GetMonitorDataAsync(string? status = null, string? paymentType = null)
    {
        AddAuth();
        var url = "machines/monitor";
        if (!string.IsNullOrEmpty(status)) url += $"?status={status}";
        if (!string.IsNullOrEmpty(paymentType)) url += (url.Contains("?") ? "&" : "?") + $"paymentType={paymentType}";
        var res = await _client.GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        return JsonConvert.DeserializeObject<List<Machine>>(await res.Content.ReadAsStringAsync());
    }

    public async Task<DashboardSummary?> GetDashboardAsync()
    {
        AddAuth();
        var res = await _client.GetAsync("dashboard/summary");
        if (!res.IsSuccessStatusCode) return null;
        return JsonConvert.DeserializeObject<DashboardSummary>(await res.Content.ReadAsStringAsync());
    }

    public async Task<List<News>?> GetNewsAsync()
    {
        AddAuth();
        var res = await _client.GetAsync("dashboard/news");
        if (!res.IsSuccessStatusCode) return null;
        return JsonConvert.DeserializeObject<List<News>>(await res.Content.ReadAsStringAsync());
    }

    public async Task<bool> CreateMachineAsync(Machine m)
    {
        AddAuth();
        var json = JsonConvert.SerializeObject(m);
        var res = await _client.PostAsync("machines", new StringContent(json, Encoding.UTF8, "application/json"));
        return res.IsSuccessStatusCode;
    }
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
