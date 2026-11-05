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
    private readonly ITenantResolver _tenantResolver;

    public AuthController(QuantarisDbContext db, IJwtTokenService jwtToken, ITenantResolver tenantResolver)
    {
        _db = db;
        _jwtToken = jwtToken;
        _tenantResolver = tenantResolver;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantCode == request.TenantCode && t.IsActive);
        if (tenant == null) return Unauthorized(new { error = "Invalid tenant" });

        _tenantResolver.SetTenant(tenant.Id, tenant.TenantCode);

        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username && u.TenantId == tenant.Id && u.IsActive);
        if (user == null) return Unauthorized(new { error = "Invalid credentials" });

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
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

        _tenantResolver.SetTenant(tenant.Id, tenant.TenantCode);

        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == request.Username && u.TenantId == tenant.Id))
            return Conflict(new { error = "Username already exists" });

        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email && u.TenantId == tenant.Id))
            return Conflict(new { error = "Email already exists" });

        var salt = GenerateSalt();
        var hash = HashPassword(request.Password, salt);

        var user = new User
        {
            TenantId = tenant.Id,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "User",
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

    public static bool VerifyPassword(string password, string storedHash, string? storedSalt = null)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;

        if (storedHash.StartsWith("pbkdf2:", StringComparison.Ordinal))
        {
            var payload = storedHash[7..];
            string saltBase64;
            string expectedHashBase64;

            var colonIdx = payload.IndexOf(':');
            if (colonIdx > 0)
            {
                saltBase64 = payload[..colonIdx];
                expectedHashBase64 = payload[(colonIdx + 1)..];
            }
            else if (!string.IsNullOrEmpty(storedSalt))
            {
                saltBase64 = storedSalt;
                expectedHashBase64 = payload;
            }
            else
            {
                return false;
            }

            try
            {
                var saltBytes = Convert.FromBase64String(saltBase64);
                var expectedBytes = Convert.FromBase64String(expectedHashBase64);
                var actualBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Legacy salted/unsalted SHA256 comparison
        var input = string.IsNullOrEmpty(storedSalt) ? password : password + storedSalt;
        var sha256Hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(sha256Hash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    public static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100000, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2:{Convert.ToBase64String(hash)}";
    }
}

public record LoginRequest(string Username, string Password, string TenantCode);
public record RegisterRequest(string Username, string Email, string Password,
    string? FirstName, string? LastName, string? Role, string TenantCode);
