using ComPlan.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class TaskStatusHistory
{
    [Key]   // чтобы EF не путался
    public int HistoryId { get; set; }

    public int ProductionTaskId { get; set; }
    public ProductionTask ProductionTask { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public int ChangedBy { get; set; }
    public User User { get; set; } = null!;
}
