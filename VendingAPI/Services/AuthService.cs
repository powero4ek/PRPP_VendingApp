using VendingAPI.Data;
using VendingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace VendingAPI.Services;

public class AuthService
{
    private readonly VendingDbContext _db;
    public AuthService(VendingDbContext db) => _db = db;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        return user;
    }
}
