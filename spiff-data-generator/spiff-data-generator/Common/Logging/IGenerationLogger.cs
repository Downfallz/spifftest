using spiff_data_generator.Common.Anomalies;

namespace spiff_data_generator.Common.Logging;

public interface IGenerationLogger : IDisposable
{
    void LogConfig(Dictionary<string, object> parameters);
    void LogAnomaly(int seq, AnomalyKind kind, AnomalySeverity severity, bool isIndividu);
    void LogProgress(int current, int total);
    void LogComplete(TimeSpan duration, long? fileSizeBytes = null);
}
