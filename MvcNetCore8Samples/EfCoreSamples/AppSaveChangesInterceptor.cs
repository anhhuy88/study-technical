using EfCoreSamples.Domains;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreSamples;

public class AppSaveChangesInterceptor : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        UpdateVersion(eventData);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        UpdateVersion(eventData);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateVersion(SaveChangesCompletedEventData eventData)
    {
        var dbContext = eventData.Context;
        var entries = dbContext.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
            (entry.Entity as BaseItem).RowVersion = (entry.Entity as BaseItem).RowVersion + 1;
        }
        dbContext.SaveChanges();
    }
}
