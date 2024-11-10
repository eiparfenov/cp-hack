namespace Server.Configuration;

public class S3Options
{
    public required string AccessKeyId { get; set; }
    public required string SecretAccessKey { get; set; }
    public required string ServiceUrl { get; set; }
    public required string BucketName { get; set; }
}