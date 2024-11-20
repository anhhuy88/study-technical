using Microsoft.EntityFrameworkCore;

namespace EfCoreSamples.Domains;

public class AppDbContext : DbContext
{
    public DbSet<ParentItem> ParentItems { get; set; }
    public DbSet<ChildItem> ChildItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Filename=E:\\Projects\\study-technical\\MvcNetCore8Samples\\EfCoreSamples\\Data\\DemoData.db")
            .AddInterceptors(new AppSaveChangesInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentItem>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(p => p.RowVersion).IsConcurrencyToken();
        });

        modelBuilder.Entity<ChildItem>(entity =>
        {
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
