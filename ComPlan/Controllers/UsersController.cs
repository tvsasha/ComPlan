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

        

        // DTO для безопасного обновления
        public class UpdateUserDto
        {
            public required string Email { get; set; }  
            public string? UserName { get; set; }
            public int? RoleId { get; set; }
            public string? Password { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.RoleId,
                    Role = new { u.Role.RoleId, u.Role.RoleName }, // вернуть объект Role
                    u.SessionToken,
                    u.SessionExpiresAt
                })
                .ToListAsync();

            return Ok(users);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
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
                    // проверяем что роль существует
                    var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == newRoleId);
                    if (!roleExists)
                        return BadRequest(new { message = "Роль не найдена" });

                    user.RoleId = newRoleId;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                // ⚠️ Пароль всё ещё хранится в plain-text
                user.Password = dto.Password;
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
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Пользователь не найден" });

            // ❌ Сбрасываем сессию перед удалением
            user.SessionToken = null;
            user.SessionExpiresAt = null;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Пользователь удалён" });
        }
    }
}
