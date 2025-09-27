using ComPlan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComPlan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) => _context = context;
        public class UpdateUserDto
        {
            public required string Email { get; set; }
            public string? UserName { get; set; }
            public int? RoleId { get; set; }
            public string? Password { get; set; }
        }

        private async Task<User?> ValidateSession(string? authHeader)
        {
            Console.WriteLine("Полученный Authorization: " + (authHeader ?? "отсутствует"));

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Console.WriteLine("Заголовок некорректный или отсутствует");
                return null;
            }

            var token = authHeader.Substring(7).Trim();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.SessionToken == token);

            if (user == null)
            {
                Console.WriteLine("Пользователь не найден по токену: " + token);
                return null;
            }

            Console.WriteLine("SessionExpiresAt: " + user.SessionExpiresAt);
            Console.WriteLine("Текущее время UTC: " + DateTime.UtcNow);

            if (user.SessionExpiresAt == null || user.SessionExpiresAt < DateTime.UtcNow)
            {
                Console.WriteLine("Сессия истекла или ExpiresAt null");
                return null;
            }

            return user;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromHeader(Name = "Authorization")] string? token)
        {
            var currentUser = await ValidateSession(token);
            if (currentUser == null)
            {
                return Unauthorized(new { message = "Сессия недействительна" });
            }

            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.RoleId,
                    Role = new { u.Role.RoleId, u.Role.RoleName }
                })
                .ToListAsync();

            Console.WriteLine("Возвращено пользователей: " + users.Count);
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromHeader(Name = "Authorization")] string? token, [FromBody] UpdateUserDto dto)
        {
            var currentUser = await ValidateSession(token);
            if (currentUser == null)
                return Unauthorized(new { message = "Сессия недействительна" });
            if (currentUser.RoleId != 1)
                return Forbid("Только администратор может редактировать пользователей");

            if (dto == null) return BadRequest(new { message = "Нет данных для обновления" });

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Пользователь не найден" });

            var changed = false;

            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                var newName = dto.UserName.Trim();
                if (user.UserName != newName)
                {
                    user.UserName = newName;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                if (user.Email != newEmail)
                {
                    if (await _context.Users.AnyAsync(u => u.Email == newEmail && u.UserId != id))
                        return BadRequest(new { message = "Email уже используется" });

                    user.Email = newEmail;
                    changed = true;
                }
            }

            if (dto.RoleId.HasValue)
            {
                var newRoleId = dto.RoleId.Value;
                if (user.RoleId != newRoleId)
                {
                    var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == newRoleId);
                    if (!roleExists)
                        return BadRequest(new { message = "Роль не найдена" });

                    user.RoleId = newRoleId;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = AuthController.HashPassword(dto.Password);
                changed = true;
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }

            var updatedUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromHeader(Name = "Authorization")] string? token)
        {
            var currentUser = await ValidateSession(token);
            if (currentUser == null)
                return Unauthorized(new { message = "Сессия недействительна" });
            if (currentUser.RoleId != 1)
                return Forbid("Только администратор может удалять пользователей");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Пользователь не найден" });

            user.SessionToken = null;
            user.SessionExpiresAt = null;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь удалён" });
        }
    }
}