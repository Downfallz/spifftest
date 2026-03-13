using System.ComponentModel.DataAnnotations;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.T5Rl3.Config;

namespace spiff_data_generator.Api;

public sealed class GenerateRequest
{
    [Range(1, 10_000_000)]
    public int NombreIndividus { get; set; } = 500;

    [Range(1, 10_000_000)]
    public int NombreLignes { get; set; } = 1000;

    [Range(1, 100_000)]
    public int BatchSize { get; set; } = 1000;

    public int Seed { get; set; }

    public string Plateforme { get; set; } = "SPIFF";
    public string CodeSysteme { get; set; } = "D10815";
    public string TypeDeclaration { get; set; } = "O";
    public string CycleProduction { get; set; } = "A";
    public string AnneeProduction { get; set; } = "2026";

    public int[] WeightsCourrierRetenu { get; set; } = [5, 95];
    public int[] WeightsImpression { get; set; } = [80, 20];
    public int[] WeightsCodeProvince { get; set; } = [70, 30];

    public bool IndicateurOntario { get; set; }
    public int NombreFeuilletParCaisse { get; set; } = 999_999;

    public bool AjouterEmetteurFourni { get; set; }
    public bool AjouterIdUnique { get; set; }
    public string PrefixeIdentificationUnique { get; set; } = "";

    public string[] Devises { get; set; } =
        ["CAD", "USD", "AUD", "DKK", "EUR", "GBP", "HKD", "JPY", "NZD"];

    public bool PrettyPrint { get; set; }

    public AnomalyConfig? Anomalies { get; set; }

    // FTP
    public string FtpPath { get; set; } = "out/ftp";

    [Range(1, 10)]
    public int FtpRetryCount { get; set; } = 3;

    [Range(1, 60)]
    public int FtpDelaiSeconds { get; set; } = 5;

    public T5Rl3Config ToConfig() => new()
    {
        Plateforme = Plateforme,
        CodeSysteme = CodeSysteme,
        TypeDeclaration = TypeDeclaration,
        CycleProduction = CycleProduction,
        AnneeProduction = AnneeProduction,
        Seed = Seed,
        NombreIndividus = NombreIndividus,
        NombreLignes = NombreLignes,
        BatchSize = BatchSize,
        WeightsCourrierRetenu = WeightsCourrierRetenu,
        WeightsImpression = WeightsImpression,
        WeightsCodeProvince = WeightsCodeProvince,
        IndicateurOntario = IndicateurOntario,
        NombreFeuilletParCaisse = NombreFeuilletParCaisse,
        AjouterEmetteurFourni = AjouterEmetteurFourni,
        AjouterIdUnique = AjouterIdUnique,
        PrefixeIdentificationUnique = PrefixeIdentificationUnique,
        Devises = Devises,
        PrettyPrint = PrettyPrint,
        Anomalies = Anomalies ?? new AnomalyConfig(),
    };
}
