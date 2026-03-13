using System.Globalization;
using spiff_data_generator.Common.Anomalies;

namespace spiff_data_generator.Common.Logging;

public sealed class FileGenerationLogger : IGenerationLogger
{
    private readonly StreamWriter _writer;
    private readonly string _logPath;

    public FileGenerationLogger(string outputDir, string filePrefix)
    {
        Directory.CreateDirectory(outputDir);
        _logPath = Path.Combine(outputDir, $"{filePrefix}.log");
        _writer = new StreamWriter(_logPath, append: false) { AutoFlush = true };
        _writer.WriteLine($"[{Now()}] === Génération démarrée ===");
    }

    public void LogConfig(Dictionary<string, object> parameters)
    {
        _writer.WriteLine($"[{Now()}] Paramètres:");
        foreach (var (key, value) in parameters)
            _writer.WriteLine($"  {key} = {value}");
        _writer.WriteLine();
    }

    public void LogAnomaly(int seq, AnomalyKind kind, AnomalySeverity severity, bool isIndividu)
    {
        string type = isIndividu ? "Individu" : "Organisation";
        _writer.WriteLine($"[{Now()}] Anomalie seq={seq} type={type} severity={severity} kind={kind}");
    }

    public void LogProgress(int current, int total)
    {
        double pct = current / (double)total * 100.0;
        _writer.WriteLine($"[{Now()}] Progress: {current}/{total} ({pct:F1}%)");
    }

    public void LogComplete(TimeSpan duration, long? fileSizeBytes = null)
    {
        _writer.WriteLine();
        _writer.WriteLine($"[{Now()}] === Génération terminée ===");
        _writer.WriteLine($"  Durée: {duration}");
        if (fileSizeBytes.HasValue)
            _writer.WriteLine($"  Taille: {fileSizeBytes.Value:N0} bytes");
        _writer.WriteLine($"  Log: {_logPath}");
    }

    public void Dispose() => _writer.Dispose();

    private static string Now() =>
        DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
}
