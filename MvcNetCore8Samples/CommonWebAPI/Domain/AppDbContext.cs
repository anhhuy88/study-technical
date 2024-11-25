using Microsoft.EntityFrameworkCore;

namespace CommonWebAPI.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileData>(entity =>
        {
            entity.ToTable($"{nameof(FileData)}s");
            entity.HasKey(e => e.Id);
        });
    }
}
