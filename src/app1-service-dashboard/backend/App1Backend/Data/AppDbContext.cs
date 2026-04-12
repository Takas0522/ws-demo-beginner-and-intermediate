using App1Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace App1Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServicePlan> ServicePlans => Set<ServicePlan>();
    public DbSet<UserMetricDaily> UserMetricDailies => Set<UserMetricDaily>();
    public DbSet<RevenueDaily> RevenueDailies => Set<RevenueDaily>();
    public DbSet<CostDaily> CostDailies => Set<CostDaily>();
    public DbSet<AbTest> AbTests => Set<AbTest>();
    public DbSet<AbTestVariant> AbTestVariants => Set<AbTestVariant>();
    public DbSet<AbTestResult> AbTestResults => Set<AbTestResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessUnit>(e => e.ToTable("business_units"));
        modelBuilder.Entity<ServiceCategory>(e => e.ToTable("service_categories"));

        modelBuilder.Entity<Service>(e =>
        {
            e.ToTable("services");
            e.HasOne(s => s.BusinessUnit).WithMany(b => b.Services).HasForeignKey(s => s.BusinessUnitId);
            e.HasOne(s => s.Category).WithMany(c => c.Services).HasForeignKey(s => s.CategoryId);
        });

        modelBuilder.Entity<ServicePlan>(e => e.ToTable("service_plans"));

        modelBuilder.Entity<UserMetricDaily>(e =>
        {
            e.ToTable("user_metric_daily");
            e.HasIndex(u => new { u.ServiceId, u.Date }).IsUnique();
        });

        modelBuilder.Entity<RevenueDaily>(e =>
        {
            e.ToTable("revenue_daily");
            e.HasOne(r => r.Plan).WithMany(p => p.Revenues).HasForeignKey(r => r.PlanId);
        });

        modelBuilder.Entity<CostDaily>(e => e.ToTable("cost_daily"));

        modelBuilder.Entity<AbTest>(e =>
        {
            e.ToTable("ab_tests");
            // winner_variant_id は循環FK回避のためナビゲーションプロパティなし
            e.Property(a => a.WinnerVariantId).HasColumnName("winner_variant_id");
        });

        modelBuilder.Entity<AbTestVariant>(e => e.ToTable("ab_test_variants"));
        modelBuilder.Entity<AbTestResult>(e => e.ToTable("ab_test_results"));
    }
}
