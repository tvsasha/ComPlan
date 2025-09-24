using ComPlan.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class PurchaseRequest
{
    [Key]  // ← можно явно добавить, но не обязательно если имя RequestId
    public int RequestId { get; set; }

    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;

    public int Quantity { get; set; }

    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CreatedBy { get; set; }
    public User User { get; set; } = null!;
}
