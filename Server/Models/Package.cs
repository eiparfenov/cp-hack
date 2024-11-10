namespace Server.Models;

public class Package
{
    public Guid Id { get; set; }
    public DateTimeOffset Time { get; set; }
    public List<Image>? Images { get; set; }
}