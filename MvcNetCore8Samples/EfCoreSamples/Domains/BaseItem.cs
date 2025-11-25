namespace EfCoreSamples.Domains;

public class BaseItem
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
    public int RowVersion { get; set; }
}
