using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User>       Users       => Set<User>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(e =>
        {
            e.ToTable("departments");
            e.HasMany(d => d.Users).WithOne(u => u.Department).HasForeignKey(u => u.DepartmentId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
