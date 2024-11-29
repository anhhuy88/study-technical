using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebMvc.Domains;
using WebMvc.Interfaces;

namespace WebMvc.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    private readonly IHttpContextAccessor _accessor;

    public UnitOfWork(AppDbContext dbContext, IHttpContextAccessor accessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException("AppDbContext");
        _accessor = accessor;
    }

    private string _userName;

    private string UserName
    {
        get
        {
            if (_userName == null)
            {
                var claimsPrincipal = _accessor.HttpContext?.User;
                var userId = claimsPrincipal?.FindFirst(ClaimTypes.Name)?.Value;
                _userName = userId ?? string.Empty;
            }
            return _userName;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        var entities = _dbContext.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

        var isConflict = entities.Where(e => e.Property(nameof(BaseItem.RowVersion)).CurrentValue?.ToString().Equals(e.Property(nameof(BaseItem.RowVersion)).OriginalValue?.ToString()) == false)
                        .Where(e => e.State != EntityState.Added)
                        .Any();

        if (isConflict)
        {
            throw new DbUpdateConcurrencyException("The record has been updated by another user.");
        }

        foreach (var entity in entities)
        {
            if (entity.Entity is BaseItem item)
            {
                var isAdded = item.Id < 1;

                if (isAdded)
                {
                    isAdded = true;
                    item.CreatedBy = UserName;
                    item.CreatedDate = DateTime.Now;
                }
                else
                {
                    item.ModifiedBy = UserName;
                    item.ModifiedDate = DateTime.Now;
                }
            }
        }

        var result = await _dbContext.SaveChangesAsync();

        entities = _dbContext.ChangeTracker.Entries();

        foreach (var entity in entities)
        {
            if (entity.Entity is BaseItem item)
            {
                var tableName = entity.Metadata.GetTableName();
                var querySQL = string.Format("UPDATE {0} SET {1} = {2} WHERE Id = {3}",
                    tableName,
                    nameof(BaseItem.RowVersion),
                    DateTime.Now.TimeOfDay.Ticks,
                    item.Id);
                await _dbContext.Database.ExecuteSqlRawAsync(querySQL);
            }
        }

        return result;
    }
}