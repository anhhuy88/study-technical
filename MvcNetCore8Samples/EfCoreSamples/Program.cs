// See https://aka.ms/new-console-template for more information
using EfCoreSamples.Domains;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");

using var dbContext = new AppDbContext();
dbContext.ParentItems.Add(new ParentItem
{
    Name = "Parent 1",
    ChildItems = new List<ChildItem>
    {
        new ChildItem { Name = "Child 1" },
        new ChildItem { Name = "Child 2" }
    }
});

await dbContext.SaveChangesAsync();

Console.ReadKey();