using Microsoft.Extensions.Logging;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Logging;

namespace spiff_data_generator.Api;

public sealed class MsLoggerGenerationLogger : IGenerationLogger
{
    private readonly ILogger _logger;

    public MsLoggerGenerationLogger(ILogger<MsLoggerGenerationLogger> logger)
    {
        _logger = logger;
    }

    public void LogConfig(Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Paramètres de génération:");
        foreach (var (key, value) in parameters)
            _logger.LogInformation("  {Key} = {Value}", key, value);
    }

    public void LogAnomaly(int seq, AnomalyKind kind, bool isIndividu)
    {
        _logger.LogInformation("Anomalie seq={Seq} type={Type} kind={Kind}",
            seq, isIndividu ? "Individu" : "Organisation", kind);
    }

    public void LogProgress(int current, int total)
    {
        _logger.LogInformation("Progress: {Current}/{Total} ({Pct:F1}%)",
            current, total, current / (double)total * 100.0);
    }

    public void LogComplete(TimeSpan duration, long? fileSizeBytes = null)
    {
        _logger.LogInformation("Génération terminée en {Duration}. Taille: {Size} bytes",
            duration, fileSizeBytes);
    }

    public void Dispose() { }
}
