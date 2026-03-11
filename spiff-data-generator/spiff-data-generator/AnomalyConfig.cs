namespace spiff_data_generator;

public class AnomalyConfig
{
    public bool Enabled { get; set; } = false;
    public AnomalyLevelConfig Bloquant { get; set; } = new();
    public AnomalyLevelConfig Importante { get; set; } = new();
    public AnomalyLevelConfig SevereImpression { get; set; } = new();
    public AnomalyLevelConfig Avertissement { get; set; } = new();
}

public class AnomalyLevelConfig
{
    public int Nombre { get; set; } = 0;
    public string[] Types { get; set; } = [];
}
