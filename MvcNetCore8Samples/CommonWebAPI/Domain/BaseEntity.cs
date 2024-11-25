namespace CommonWebAPI.Domain;

public class BaseEntity
{
    public string Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
    public int RowVersion { get; set; }
}
