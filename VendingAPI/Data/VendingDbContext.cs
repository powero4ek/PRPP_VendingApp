using Microsoft.EntityFrameworkCore;
using VendingAPI.Models;

namespace VendingAPI.Data;

public class VendingDbContext : DbContext
{
    public VendingDbContext(DbContextOptions<VendingDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Modem> Modems => Set<Modem>();
    public DbSet<News> News => Set<News>();
    public DbSet<Incassation> Incassations => Set<Incassation>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<Protocol> Protocols => Set<Protocol>();
    public DbSet<UserModel> UserModels => Set<UserModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("vending");

        // ⬇️ Имена таблиц в нижнем регистре
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Machine>().ToTable("machines");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<Sale>().ToTable("sales");
        modelBuilder.Entity<Maintenance>().ToTable("maintenance");
        modelBuilder.Entity<Company>().ToTable("companies");
        modelBuilder.Entity<Modem>().ToTable("modems");
        modelBuilder.Entity<News>().ToTable("news");
        modelBuilder.Entity<Incassation>().ToTable("incassations");
        modelBuilder.Entity<ServiceRequest>().ToTable("servicerequests");
        modelBuilder.Entity<StatusHistory>().ToTable("statushistory");
        modelBuilder.Entity<Protocol>().ToTable("protocols");
        modelBuilder.Entity<UserModel>().ToTable("usermodels");

        // ⬇️ Имена столбцов тоже в нижнем регистре
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(property.Name.ToLowerInvariant());
            }
        }
    }
}