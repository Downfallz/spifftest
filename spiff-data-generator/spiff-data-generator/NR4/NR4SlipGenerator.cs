using spiff_data_generator.Common;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.NR4;

public sealed class NR4SlipGenerator : ISlipGenerator
{
    private readonly GeneratorConfig _config;
    private readonly IRandomService _random;
    private readonly IEnumerable<ISlipBuilder<NR4SlipContext>> _builders;
    private readonly IAnomalyService _anomalyService;
    private readonly IGenerationLogger _logger;

    public NR4SlipGenerator(
        GeneratorConfig config,
        IRandomService random,
        IEnumerable<ISlipBuilder<NR4SlipContext>> builders,
        IAnomalyService anomalyService,
        IGenerationLogger logger)
    {
        _config = config;
        _random = random;
        _builders = builders;
        _anomalyService = anomalyService;
        _logger = logger;
    }

    public Dictionary<string, object> Generate(int seq)
    {
        var context = BuildContext(seq);

        var builder = _builders.FirstOrDefault(b => b.CanBuild(context))
            ?? throw new InvalidOperationException($"No builder found for seq {seq}");

        var root = builder.Build(context);

        if (_config.AjouterIdUnique)
        {
            var info = (Dictionary<string, object>)root["information"];
            info["numIdentificationUnique"] = $"{_config.PrefixeIdentificationUnique}{seq}";
        }

        var anomaly = _anomalyService.GetAnomalyForSequence(seq);
        if (anomaly.HasValue)
        {
            var (kind, severity) = anomaly.Value;
            _anomalyService.Apply(root, kind, context.IsIndividu);
            _logger.LogAnomaly(seq, kind, severity, context.IsIndividu);
        }

        return root;
    }

    private NR4SlipContext BuildContext(int seq)
    {
        // TODO: Implement NR4-specific context building logic
        throw new NotImplementedException("NR4 context building not yet implemented");
    }
}
