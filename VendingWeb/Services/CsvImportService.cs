using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using VendingWeb.Data;
using VendingWeb.Models;

namespace VendingWeb.Services;

public class CsvImportService
{
    private readonly VendingDbContext _db;
    public CsvImportService(VendingDbContext db) => _db = db;

    public async Task<ImportResult> ImportMachinesAsync(Stream csvStream)
    {
        var result = new ImportResult();
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null });
        var records = csv.GetRecords<MachineCsvRecord>().ToList();

        foreach (var rec in records)
        {
            result.Total++;
            if (string.IsNullOrWhiteSpace(rec.Location) || string.IsNullOrWhiteSpace(rec.Model) || string.IsNullOrWhiteSpace(rec.SerialNumber))
            {
                result.Errors.Add($"Строка {result.Total}: отсутствуют обязательные поля");
                continue;
            }
            if (_db.Machines.Any(m => m.SerialNumber == rec.SerialNumber || m.InventoryNumber == rec.InventoryNumber))
            {
                result.Errors.Add($"Строка {result.Total}: дублирование серийного/инвентарного номера");
                continue;
            }
            var machine = new Machine
            {
                Location = rec.Location,
                Model = rec.Model,
                PaymentType = rec.PaymentType ?? "с оплатой картой",
                SerialNumber = rec.SerialNumber,
                InventoryNumber = rec.InventoryNumber ?? $"INV-{DateTime.Now:yyyy}-{new Random().Next(100,999)}",
                ManufactureDate = DateTime.TryParse(rec.ManufactureDate, out var md) ? md : DateTime.Now.AddYears(-1),
                DateOfCommissioning = DateTime.TryParse(rec.DateOfCommissioning, out var dc) ? dc : DateTime.Now,
                MachineStatus = rec.MachineStatus ?? "Работает",
                Country = rec.Country ?? "Россия",
                DateAdded = DateTime.Now
            };
            _db.Machines.Add(machine);
            result.Imported++;
        }
        await _db.SaveChangesAsync();
        return result;
    }
}

public class MachineCsvRecord
{
    public string? Location { get; set; }
    public string? Model { get; set; }
    public string? PaymentType { get; set; }
    public string? SerialNumber { get; set; }
    public string? InventoryNumber { get; set; }
    public string? ManufactureDate { get; set; }
    public string? DateOfCommissioning { get; set; }
    public string? MachineStatus { get; set; }
    public string? Country { get; set; }
}

public class ImportResult
{
    public int Total { get; set; }
    public int Imported { get; set; }
    public List<string> Errors { get; set; } = new();
}
