using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ExcursionsStore.API.Data;
using ExcursionsStore.API.Models;
using ExcursionsStore.API.Models.Auth;

namespace ExcursionsStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (await _context.Clients.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var client = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                PhoneNumber = request.PhoneNumber,
                RegistrationDate = DateTime.UtcNow,
                Role = "User" // Все новые пользователи - User
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(client);

            return new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName ?? string.Empty,
                    Role = client.Role
                }
            };
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Email == request.Email);

            if (client == null || !BCrypt.Net.BCrypt.Verify(request.Password, client.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный email или пароль" });
            }

            var token = GenerateJwtToken(client);

            return new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = new UserInfo
                {
                    Id = client.Id,
                    Email = client.Email,
                    FirstName = client.FirstName,
                    LastName = client.LastName ?? string.Empty,
                    Role = client.Role
                }
            };
        }

        // GET: api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> GetProfile()
        {
            var clientId = GetClientIdFromToken();
            if (clientId == null) return Unauthorized();

            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return NotFound();

            return new UserInfo
            {
                Id = client.Id,
                Email = client.Email,
                FirstName = client.FirstName,
                LastName = client.LastName ?? string.Empty,
                Role = client.Role
            };
        }

        private string GenerateJwtToken(Client client)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var fullName = $"{client.FirstName} {client.LastName}".Trim();
            if (string.IsNullOrEmpty(client.LastName))
            {
                fullName = client.FirstName;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Name, fullName),
                new Claim(ClaimTypes.Role, client.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private int? GetClientIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}