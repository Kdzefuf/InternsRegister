using InternsRegister.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternsRegister.Persistence;

public class InternsRegisterDbContext : DbContext
{
    public InternsRegisterDbContext(DbContextOptions<InternsRegisterDbContext> options)
        : base(options) { }

    public DbSet<Intern> Interns => Set<Intern>();
    public DbSet<Direction> Directions => Set<Direction>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToLower());
            }
        }

        modelBuilder.Entity<Intern>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone).IsUnique().HasFilter("Phone IS NOT NULL");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(12);
        });

        modelBuilder.Entity<Direction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}