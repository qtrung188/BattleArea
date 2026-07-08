using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.Exceptions;
using BattleArenaBackendAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BattleArenaBackendAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<User> RegisterAsync(string username, string password)
        {
            var exists = await _db.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                throw new ConflictException("Username is already taken.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Gold = 1000,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

            // Whether the username is unknown or the password is wrong, surface the
            // exact same error so a client cannot probe which usernames exist.
            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new BadRequestException("Invalid username or password.");
            }

            return GenerateToken(user);
        }

        public string GenerateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var secret = jwt["Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];
            var expiryMinutes = int.TryParse(jwt["ExpiryMinutes"], out var m) ? m : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
