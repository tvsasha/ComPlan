namespace ComPlan.Models
{
    public class Component
    {
        public int ComponentId { get; set; }
        public string Name { get; set; } = null!;
        public int StockQuantity { get; set; }

        public ICollection<TaskComponent> TaskComponents { get; set; } = new List<TaskComponent>();
        public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
    }

}
