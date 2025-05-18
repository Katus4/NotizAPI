using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using NotizApi.Data;
using NotizApi.Models;
using NotizApi.Models.Dtos;

namespace NotizApi.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly NotizContext _db;
        private readonly IConfiguration _cfg;

        public AuthController(NotizContext db, IConfiguration cfg)
        {
            _db  = db;
            _cfg = cfg;
        }

        private (byte[] hash, byte[] salt) HashPassword(string password)
        {
            var salt   = RandomNumberGenerator.GetBytes(16);
            var pepper = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
            var pwd    = Encoding.UTF8.GetBytes(password).Concat(pepper).ToArray();

            var hash = KeyDerivation.Pbkdf2(
                password: pwd, salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000, numBytesRequested: 32);

            return (hash, salt);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Benutzername existiert bereits");

            var (hash, salt) = HashPassword(dto.Password);
            var user = new User
            {
                Username     = dto.Username,
                Email        = dto.Email,
                PasswordHash = hash,
                Salt         = salt
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { user.Id, user.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Unauthorized();

            var pepper   = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
            var pwdBytes = Encoding.UTF8.GetBytes(dto.Password).Concat(pepper).ToArray();

            var hashCheck = KeyDerivation.Pbkdf2(
                password: pwdBytes, salt: user.Salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000, numBytesRequested: 32);

            if (!hashCheck.SequenceEqual(user.PasswordHash))
                return Unauthorized();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
