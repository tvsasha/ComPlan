namespace ComPlan.Models
{
    public class ProductionTask
    {
        public int ProductionTaskId { get; set; }
        public string TaskName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Status { get; set; } = "Planned";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskComponent> Components { get; set; } = new List<TaskComponent>();
        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public ICollection<TaskStatusHistory> StatusHistories { get; set; } = new List<TaskStatusHistory>();
    }

}
