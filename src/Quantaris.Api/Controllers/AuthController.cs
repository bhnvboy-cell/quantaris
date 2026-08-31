using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quantaris.Core.Entities;
using Quantaris.Infrastructure.Data;
using Quantaris.Infrastructure.Services;

namespace Quantaris.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly QuantarisDbContext _db;
    private readonly IJwtTokenService _jwtToken;

    public AuthController(QuantarisDbContext db, IJwtTokenService jwtToken)
    {
        _db = db;
        _jwtToken = jwtToken;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantCode == request.TenantCode && t.IsActive);
        if (tenant == null) return Unauthorized(new { error = "Invalid tenant" });

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Username && u.TenantId == tenant.Id && u.IsActive);
        if (user == null) return Unauthorized(new { error = "Invalid credentials" });

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid credentials" });

        user.LastLoginOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _jwtToken.GenerateToken(user, tenant);

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role,
                tenantCode = tenant.TenantCode,
                tenantName = tenant.TenantName
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantCode == request.TenantCode && t.IsActive);
        if (tenant == null) return BadRequest(new { error = "Invalid tenant" });

        if (await _db.Users.AnyAsync(u => u.Username == request.Username && u.TenantId == tenant.Id))
            return Conflict(new { error = "Username already exists" });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.TenantId == tenant.Id))
            return Conflict(new { error = "Email already exists" });

        var salt = GenerateSalt();
        var hash = HashPassword(request.Password, salt);

        var user = new User
        {
            TenantId = tenant.Id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role ?? "Viewer",
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwtToken.GenerateToken(user, tenant);
        return CreatedAtAction(nameof(Login), new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var tenantCode = User.FindFirst("TenantCode")?.Value;
        return Ok(new { userId, tenantCode, username = User.Identity?.Name });
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants()
    {
        var tenants = await _db.Tenants
            .Where(t => t.IsActive)
            .Select(t => new { t.Id, t.TenantCode, t.TenantName, t.PlanType })
            .ToListAsync();
        return Ok(tenants);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (storedHash.StartsWith("pbkdf2:"))
        {
            var expectedHash = storedHash[7..];
            var userHash = HashPassword(password, Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(16)));
            // For PBKDF2 we need the actual salt from storage
            return false; // simplified - need salt
        }
        // Legacy SHA256 comparison
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password))) == storedHash;
    }

    private static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return $"pbkdf2:{Convert.ToBase64String(hash)}";
    }
}

public record LoginRequest(string Username, string Password, string TenantCode);
public record RegisterRequest(string Username, string Email, string Password,
    string? FirstName, string? LastName, string? Role, string TenantCode);
