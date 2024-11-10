namespace Server.Models;

public class Image
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Path { get; set; } = default!;

    public string Url { get; set; } = default!;
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Guid PackageId { get; set; }
    public Package? Package { get; set; }

    public MlResult MlResult { get; set; } = default!;
}