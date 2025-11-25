namespace WebMvc.Domains;

public static class InitializeData
{
    public static async Task SeedDataAsync(this AppDbContext dbContext)
    {
        await Task.CompletedTask;
    }
}
