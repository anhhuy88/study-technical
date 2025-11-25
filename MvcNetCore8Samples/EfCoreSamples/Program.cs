// See https://aka.ms/new-console-template for more information
using EfCoreSamples.Domains;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Hello, World!");

//using var dbContext = new AppDbContext();
//var parent = new ParentItem
//{
//    Name = "Parent 1",
//    ChildItems = new List<ChildItem>
//    {
//        new ChildItem { Name = "Child 1" },
//        new ChildItem { Name = "Child 2" }
//    }
//};
//dbContext.ParentItems.Add(parent);

//var fileInfo = new FileInfo("E:\\Projects\\study-technical\\MvcNetCore8Samples\\readme.md");
//var ms = new MemoryStream();
//await fileInfo.OpenRead().CopyToAsync(ms);

//var fileData = new FileData();
//fileData.Id = Guid.NewGuid().ToString();
//fileData.FileName = fileInfo.Name;
//fileData.Data = ms.ToArray();

//dbContext.Set<FileData>().Add(fileData);

//await dbContext.SaveChangesAsync();

//using var dbContext2 = new AppDbContext();
//var parent2 = await dbContext2.ParentItems.Where(p => p.Id == parent.Id).FirstOrDefaultAsync();
//parent2.RowVersion = parent2.RowVersion + 1;
//await dbContext2.SaveChangesAsync();

var parentItemId = 5;

ParentItem parentItem = null;
using (var dbContext = new AppDbContext())
{
    parentItem = await dbContext.ParentItems.Where(p => p.Id == parentItemId).FirstOrDefaultAsync();

    parentItem.CreatedDate = DateTime.Now;
    await dbContext.SaveChangesAsync();
    await dbContext.Database.ExecuteSqlRawAsync("UPDATE ParentItems SET RowVersion = RowVersion + 1 WHERE Id = {0}", parentItemId);

    parentItem.CreatedDate = DateTime.Now;
    await dbContext.SaveChangesAsync();
}

Console.ReadKey();
