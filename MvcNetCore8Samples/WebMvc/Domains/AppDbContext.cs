using Microsoft.EntityFrameworkCore;

namespace WebMvc.Domains;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentItem>(entity =>
        {
            entity.ToTable($"{nameof(ParentItem)}s");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(p => p.RowVersion).IsConcurrencyToken();
        });

        modelBuilder.Entity<ChildItem>(entity =>
        {
            entity.ToTable($"{nameof(ChildItem)}s");

            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);

            entity.Property(p => p.RowVersion).IsConcurrencyToken();

            entity.HasOne(d => d.Parent)
                .WithMany(p => p.ChildItems)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChildItems_ParentItems");
        });
    }
}
