namespace ComPlan.DTOs
{
    public class RegisterDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }  
        public required string Password { get; set; }
        public int RoleId { get; set; }
    }



    public class LoginDto
    {
        public required string Email { get; set; }
        public string Password { get; set; } = null!;
    }
}
