using System.Globalization;

using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T5Rl3.Generation;

public sealed class SlipGenerator : ISlipGenerator
{
    private readonly T5Rl3Config _config;
    private readonly IRandomService _random;
    private readonly IEnumerable<ISlipBuilder> _builders;
    private readonly IAnomalyService _anomalyService;
    private readonly IGenerationLogger _logger;

    public SlipGenerator(
        T5Rl3Config config,
        IRandomService random,
        IEnumerable<ISlipBuilder> builders,
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

        // Ajout émetteur fourni au feuillet
        if (_config.AjouterEmetteurFourni)
        {
            var info = (Dictionary<string, object>)root["information"];
            var parties = (List<object>)info["parties"];
            var party = (Dictionary<string, object>)parties[0];
            var identification = (List<object>)party["identificationPartie"];
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 5,
                ["numIdentificationPartie"] = context.NumTransit
            });
        }

        // Ajout identification unique du feuillet (règle RS-IMPORT-CORRECTIF)
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
