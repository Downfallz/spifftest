using System.Globalization;

using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Random;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T5Rl3.Generation;

public sealed class SlipGenerator : ISlipGenerator
{
    private readonly T5Rl3Config _config;
    private readonly IRandomService _random;
    private readonly IEnumerable<ISlipBuilder> _builders;
    private readonly IAnomalyService _anomalyService;

    public SlipGenerator(
        T5Rl3Config config,
        IRandomService random,
        IEnumerable<ISlipBuilder> builders,
        IAnomalyService anomalyService)
    {
        _config = config;
        _random = random;
        _builders = builders;
        _anomalyService = anomalyService;
    }

    public Dictionary<string, object> Generate(int seq)
    {
        var context = BuildContext(seq);

        var builder = _builders.FirstOrDefault(b => b.CanBuild(context))
            ?? throw new InvalidOperationException($"No builder found for seq {seq}");

        var root = builder.Build(context);

        var anomaly = _anomalyService.GetAnomalyForSequence(seq);
        if (anomaly.HasValue)
            _anomalyService.Apply(root, anomaly.Value, context.IsIndividu);

        return root;
    }

    private SlipContext BuildContext(int seq)
    {
        bool isIndividu = seq <= _config.NombreIndividus;

        string province = _random.WeightedChoice(
            new[] { "QC", "Autre" }, _config.WeightsCodeProvince);

        if (province == "Autre")
        {
            province = _config.IndicateurOntario
                ? "ON"
                : _random.RandomChoice(new[] { "AB", "BC", "MB", "NB", "NS", "NL", "PE", "SK", "NT", "NU", "YT", "ON" });
        }

        bool isQc = province == "QC";
        string typImpression = _random.WeightedChoice(new[] { "PN", "N" }, _config.WeightsImpression);
        bool holdMail = _random.WeightedChoice(new[] { true, false }, _config.WeightsCourrierRetenu);

        int transitIndex = (seq - 1) / _config.NombreFeuilletParCaisse;
        var transitArray = _config.IndicateurOntario
            ? Constants.TransitNumbersOntario
            : Constants.TransitNumbers;
        string numTransit = transitArray[transitIndex % transitArray.Length];

        string numCompte = (seq % 999_999 == 0)
            ? "999999"
            : (seq % 999_999).ToString("D6", CultureInfo.InvariantCulture);

        return new SlipContext(
            NumTransit: numTransit,
            NumCompte: numCompte,
            Province: province,
            IsQc: isQc,
            Langue: isQc ? "F" : "A",
            Pays: "CAN",
            TypImpression: typImpression,
            HoldMail: holdMail,
            Devise: _random.RandomChoice(_config.Devises),
            Case13: _random.RandomDecimal(1, 8, 2),
            CaseD: _random.RandomDecimal(1, 8, 2),
            IsIndividu: isIndividu);
    }
}
