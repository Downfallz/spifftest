namespace spiff_data_generator.Common.Anomalies;

public interface IAnomalyService
{
    AnomalyKind? GetAnomalyForSequence(int seq);
    void Apply(Dictionary<string, object> root, AnomalyKind kind, bool isIndividu);
}
