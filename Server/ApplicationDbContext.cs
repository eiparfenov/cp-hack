using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server;

public class ApplicationDbContext: DbContext
{
    public DbSet<Package> Packages { get; set; }
    public DbSet<Image> Images { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jsonOptions = new JsonSerializerOptions();
        modelBuilder.Entity<Package>(builder =>
        {
            builder.ToTable("package");
        });
        modelBuilder.Entity<Image>(builder =>
        {
            builder.ToTable("image");
            builder
                .Property(image => image.MlResult)
                .HasColumnType("jsonb")
                .HasConversion(v => JsonSerializer.Serialize(v, jsonOptions),
                    str => JsonSerializer.Deserialize<MlResult>(str, jsonOptions) ?? new MlResult());
        });
    }
}