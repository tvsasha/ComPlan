using ComPlan.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Component> Components => Set<Component>();
    public DbSet<ProductionTask> ProductionTasks => Set<ProductionTask>();
    public DbSet<TaskComponent> TaskComponents => Set<TaskComponent>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<TaskStatusHistory> TaskStatusHistories => Set<TaskStatusHistory>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // связь многие-ко-многим между задачами и компонентами
        modelBuilder.Entity<TaskComponent>()
            .HasKey(tc => new { tc.ProductionTaskId, tc.ComponentId });

        // связь многие-ко-многим между задачами и исполнителями
        modelBuilder.Entity<TaskAssignment>()
            .HasKey(ta => new { ta.ProductionTaskId, ta.UserId });
    }
}
