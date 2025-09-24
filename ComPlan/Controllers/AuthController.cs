using ComPlan.DTOs;
using ComPlan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ComPlan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Хэширование пароля
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }


        // 📌 Регистрация
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Заполните все поля" });

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Пользователь с такой почтой уже существует" });

            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return BadRequest(new { message = "Роль не найдена" });

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = HashPassword(dto.Password), // сохраняем хэш
                RoleId = dto.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Регистрация прошла успешно" });
        }

        // 📌 Логин
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Заполните Email и пароль" });

            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || user.Password != HashPassword(dto.Password))
                return Unauthorized(new { message = "Неверный Email или пароль" });

            return Ok(new
            {
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                role = new { user.Role.RoleId, user.Role.RoleName }
            });
        }

        // 📌 Logout (удаляет сессию)
        [HttpPost("logout/{userId}")]
        public async Task<IActionResult> Logout(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "Пользователь не найден" });

            user.SessionToken = null;
            user.SessionExpiresAt = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Сессия завершена" });
        }
    }
}
