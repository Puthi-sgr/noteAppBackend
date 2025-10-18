using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NoteApp.Domain;
using NoteApp.Dtos;
using NoteApp.Repositories;

namespace NoteApp.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository users, IConfiguration config)
        {
            _users = users;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(dto.Email));
            if (string.IsNullOrEmpty(dto.Password)) throw new ArgumentException("Password is required.", nameof(dto.Password));

            var existing = await _users.GetByEmailAsync(email);
            if (existing is not null)
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                Email = email
            };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            var id = await _users.CreateAsync(user);
            user.Id = id;
            user.CreatedAt = DateTime.UtcNow;

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _users.GetByEmailAsync(email);
            if (user is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials.");

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var key = _config["Jwt:Key"]!;
            var minutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(minutes);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new AuthResponseDto
            {
                Token = token,
                ExpiresInSeconds = (int)TimeSpan.FromMinutes(minutes).TotalSeconds
            };
        }
    }
}
