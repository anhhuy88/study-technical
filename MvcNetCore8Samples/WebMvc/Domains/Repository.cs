using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Security.Claims;

namespace WebMvc.Domains;

public interface IRepository<T> where T : class, new()
{
    IQueryable<T> Query();

    IQueryable<T> Query(Expression<Func<T, bool>> predicate);

    IQueryable<T> QueryTracking();

    IQueryable<T> QueryTracking(Expression<Func<T, bool>> predicate);
    void Add(T entity);

    Task<EntityEntry<T>> AddAsync(T entity);

    Task EditAsync(T entity);

    Task DeleteAsync(T entity);

    Task UpdateRangeAsync(IEnumerable<T> entities);

    Task InsertRangeAsync(IEnumerable<T> entities);

    Task DeleteRangeAsync(IEnumerable<T> entities);
}

public class Repository<T> : IRepository<T> where T : BaseItem, new()
{
    private readonly AppDbContext _dbContext;

    private readonly IHttpContextAccessor _accessor;

    public Repository(AppDbContext dbContext, IHttpContextAccessor accessor)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException("entitiesContext");

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

    private DbSet<T> _entities;

    protected DbSet<T> Entities
    {
        get
        {
            if (_entities == null)
            {
                _entities = _dbContext.Set<T>();
            }
            return _entities;
        }
    }

    public IQueryable<T> Query()
    {
        return Entities.AsNoTracking();
    }

    public IQueryable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return Query().Where(predicate);
    }

    public IQueryable<T> QueryTracking()
    {
        return Entities;
    }

    public IQueryable<T> QueryTracking(Expression<Func<T, bool>> predicate)
    {
        return QueryTracking().Where(predicate);
    }

    public void Add(T entity)
    {
        _dbContext.Add(entity);
    }

    public async Task<EntityEntry<T>> AddAsync(T entity)
    {
        return await _dbContext.AddAsync(entity);
    }

    public Task EditAsync(T entity)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        Entities.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || entities.Count() == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            await EditAsync(entity);
        }
    }

    public async Task InsertRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || entities.Count() == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            //SaveBase(entity, true);
            await AddAsync(entity);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || entities.Count() == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            await DeleteAsync(entity);
        }
    }
}
