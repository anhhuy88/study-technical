namespace WebMvc.Domains;

public class ChildItem : BaseItem
{
    public int ParentId { get; set; }

    public string Name { get; set; }

    public ParentItem Parent { get; set; }
}

public class ParentItem : BaseItem
{
    public string Name { get; set; }

    public List<ChildItem> ChildItems { get; set; }
}
