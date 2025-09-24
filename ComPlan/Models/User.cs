namespace ComPlan.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public string? SessionToken { get; set; }
        public DateTime? SessionExpiresAt { get; set; }

        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskStatusHistory> StatusHistories { get; set; } = new List<TaskStatusHistory>();
        public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
    }
}
