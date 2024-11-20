namespace EfCoreSamples.Domains;

public class ParentItem : BaseItem
{
    public string Name { get; set; }

    public List<ChildItem> ChildItems { get; set; }
}
