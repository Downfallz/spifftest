namespace spiff_data_generator.Common.Anomalies;

public sealed class AnomalyConfig
{
    public bool Enabled { get; set; }
    public AnomalyLevelConfig Bloquant { get; set; } = new();
    public AnomalyLevelConfig Importante { get; set; } = new();
    public AnomalyLevelConfig SevereImpression { get; set; } = new();
    public AnomalyLevelConfig Avertissement { get; set; } = new();
}

public sealed class AnomalyLevelConfig
{
    public int Nombre { get; set; }
    public AnomalyKind[] Types { get; set; } = [];
}
