using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Api;

public static class Archive
{
    public static RouteGroupBuilder MapArchive(this RouteGroupBuilder builder)
    {
        builder.MapGet("list_packages", async ([FromServices] ApplicationDbContext db) =>
        {
            var packages = await db.Packages
                .AsNoTracking()
                .Include(p => p.Images)
                .ToArrayAsync();
            return Results.Json(new
            {
                Packages = packages
                    .Select(p => new PackageDto()
                    {
                        Id = p.Id,
                        Time = p.Time.ToUnixTimeMilliseconds(),
                        GoodDetections = p.Images!.ToArray().SelectMany(i => i.MlResult.Detections!).Count(t => t.Class == 1),
                        BadDetections = p.Images!.ToArray().SelectMany(i => i.MlResult.Detections!).Count(t => t.Class == 0),
                        MappedPictures = p.Images!.ToArray().Count(i => i.MlResult.IsDetected)
                    })
                    .OrderByDescending(p => p.Time)
                    .ToArray()
            });
        });
        
        builder.MapGet("get_package", async ([FromQuery] Guid packageId, [FromServices] ApplicationDbContext db) =>
        {
            var package = await db.Packages
                .Include(p => p.Images)
                .SingleOrDefaultAsync(p => p.Id == packageId);
            if (package is null) return Results.NotFound();

            var images = package.Images!.Select(i => new ImageDto()
            {
                Id = i.Id,
                Path = i.Path,
                Title = i.Title,
                MlResult = i.MlResult,
                Url = i.Url,
                Width = i.Width,
                Height = i.Height,
            })
            .ToArray();
            return Results.Json(new
            {
                GoodImages = images.Where(i => i.MlResult.Detections!.Any(d => d.Class == 1)).ToArray(),
                BadImages = images.Where(i => i.MlResult.Detections!.All(d => d.Class == 0) && i.MlResult.IsDetected).ToArray(),
                UndetectedImages = images.Where(i => !i.MlResult.IsDetected).ToArray(),
            });
        });
        
        builder.MapGet("get_report", async ([FromQuery] Guid packageId, [FromServices] ApplicationDbContext db) =>
        {
            var package = await db.Packages
                .Include(p => p.Images)
                .SingleOrDefaultAsync(p => p.Id == packageId);
            if (package is null) return Results.NotFound();

            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await using var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture){Delimiter = ";"}, true);

            csvWriter.WriteHeader(typeof(ReportRow));
            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(package.Images!.SelectMany(i =>
                i.MlResult.Detections!.Select(d => new ReportRow()
                    { Title = i.Title, Bbox = $"[{d.Xc}, {d.Yc}, {d.W}, {d.H}]", Class = d.Class })));
            await csvWriter.FlushAsync();
            var bytes = stream.ToArray();
            return Results.File(bytes, "application/json", $"Анализ изображений {package.Time:yyyy/dd/MM}.csv");
        });
        return builder;
    }

    private class PackageDto
    {
        public Guid Id { get; set; }
        public long Time { get; set; }
        public int MappedPictures { get; set; }
        public int GoodDetections { get; set; }
        public int BadDetections { get; set; }
    }

    private class ImageDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public MlResult MlResult { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }

    private class ReportRow
    {
        public string Title { get; set; } = default!;
        public string Bbox { get; set; } = default!;
        public int Class { get; set; }
    }
}