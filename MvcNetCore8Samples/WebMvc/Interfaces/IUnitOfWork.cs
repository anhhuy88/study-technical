namespace WebMvc.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}