// See https://aka.ms/new-console-template for more information
using EfCoreSamples.Domains;

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

var fileInfo = new FileInfo("E:\\Projects\\study-technical\\MvcNetCore8Samples\\readme.md");
var ms = new MemoryStream();
await fileInfo.OpenRead().CopyToAsync(ms);

var fileData = new FileData();
fileData.Id = Guid.NewGuid().ToString();
fileData.FileName = fileInfo.Name;
fileData.Data = ms.ToArray();

dbContext.Set<FileData>().Add(fileData);

await dbContext.SaveChangesAsync();

Console.ReadKey();
