namespace EfCoreSamples.Domains;
public class ChildItem : BaseItem
{
    public int ParentId { get; set; }
    public string Name { get; set; }
    public ParentItem Parent { get; set; }
}
