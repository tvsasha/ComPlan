using ComPlan.Models;

public class TaskAssignment
{
    public int ProductionTaskId { get; set; }   // PK (composite)
    public ProductionTask ProductionTask { get; set; } = null!;

    public int UserId { get; set; }             // PK (composite)
    public User User { get; set; } = null!;
}
