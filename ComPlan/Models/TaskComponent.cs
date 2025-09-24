using ComPlan.Models;

public class TaskComponent
{
    public int ProductionTaskId { get; set; }
    public ProductionTask ProductionTask { get; set; } = null!;

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;

    public int RequiredQuantity { get; set; }
}
