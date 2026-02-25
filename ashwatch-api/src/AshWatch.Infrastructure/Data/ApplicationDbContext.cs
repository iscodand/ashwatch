using AshWatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AshWatch.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnType("uuid").ValueGeneratedNever();
            entity.Property(x => x.AuthorId).HasColumnType("uuid");
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnType("uuid").ValueGeneratedNever();
            entity.Property(x => x.TenantId).HasColumnType("uuid");
            entity.Property(x => x.AuthorId).HasColumnType("uuid");
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnType("uuid").ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Username).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Password).HasMaxLength(255).IsRequired();
            entity.Property(x => x.RefreshToken).HasMaxLength(255).IsRequired(false);
        });
    }
}
