using System.Globalization;
using System.IO.Compression;
using Newtonsoft.Json;

namespace spiff_data_generator;

public sealed class ZipExporter : IZipExporter
{
    private readonly T5Rl3Config _config;
    private readonly ISlipGenerator _generator;

    public ZipExporter(T5Rl3Config config, ISlipGenerator generator)
    {
        _config = config;
        _generator = generator;
    }

    public void ExportToFile()
    {
        var currentDate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var filePrefix = $"{_config.GetOutputPrefix()}_{currentDate}01";
        var zipPath = Path.Combine(_config.OutputDir, $"{filePrefix}.zip");
        Directory.CreateDirectory(_config.OutputDir);

        using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        ExportToStream(fs, currentDate);
        Console.WriteLine($"ZIP généré: {zipPath}");
    }

    public void ExportToStream(Stream output, string? yyyymmdd = null)
    {
        int jsonFileIndex = 1;
        using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
        var startTime = DateTime.Now;

        for (int i = 0; i < _config.NombreLignes; i += _config.BatchSize)
        {
            int startSeq = i + 1;
            int endSeq = Math.Min(startSeq + _config.BatchSize, 1 + _config.NombreLignes);

            WriteBatch(zip, jsonFileIndex, startSeq, endSeq);
            jsonFileIndex++;
            PrintProgress(endSeq - 1, _config.NombreLignes);
        }

        var endTime = DateTime.Now;
        Console.WriteLine($"Génération terminée à: {endTime:HH:mm:ss}");
        Console.WriteLine($"Durée totale: {endTime - startTime}");
    }

    private void WriteBatch(ZipArchive zip, int fileIndex, int startSeq, int endSeq)
    {
        var entry = zip.CreateEntry(
            $"{_config.GetOutputPrefix()}_{fileIndex}.json",
            CompressionLevel.Optimal);

        using var entryStream = entry.Open();
        using var sw = new StreamWriter(entryStream);
        using var jw = new JsonTextWriter(sw)
        {
            Formatting = _config.PrettyPrint ? Formatting.Indented : Formatting.None
        };

        var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            StringEscapeHandling = StringEscapeHandling.Default
        });

        jw.WriteStartArray();
        for (int seq = startSeq; seq < endSeq; seq++)
        {
            var obj = _generator.Generate(seq);
            serializer.Serialize(jw, obj);
        }
        jw.WriteEndArray();
    }

    private static void PrintProgress(int current, int total)
    {
        double pct = current / (double)total * 100.0;
        Console.WriteLine($"Progress: {current}/{total} ({pct:F2}%)");
    }
}
