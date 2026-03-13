using spiff_data_generator.Common.Anomalies;

namespace spiff_data_generator.Common.Logging;

public sealed class NullGenerationLogger : IGenerationLogger
{
    public void LogConfig(Dictionary<string, object> parameters) { }
    public void LogAnomaly(int seq, AnomalyKind kind, AnomalySeverity severity, bool isIndividu) { }
    public void LogProgress(int current, int total) { }
    public void LogComplete(TimeSpan duration, long? fileSizeBytes = null) { }
    public void Dispose() { }
}
