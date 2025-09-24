namespace ComPlan.Models
{
    public class Register
    {
        public class RegisterRequest
        {
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public int RoleId { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
            public UserDto User { get; set; }
        }

        public class UserDto
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public int RoleId { get; set; }
        }

    }
}
