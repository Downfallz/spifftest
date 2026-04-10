namespace spiff_data_generator.Common.Anomalies;

public sealed class AnomalyConfig
{
    public bool Enabled { get; set; }
    public AnomalyLevelConfig Bloquant { get; set; } = new();
    public AnomalyLevelConfig Importante { get; set; } = new();
    public AnomalyLevelConfig SevereImpression { get; set; } = new();
    public AnomalyLevelConfig Avertissement { get; set; } = new();

    public static readonly AnomalyLevelConfig DefaultBloquant = new()
    {
        Nombre = 6,
        Types = new[] {
            AnomalyKind.NomBeneficiaireManquant,
            AnomalyKind.PrenomBeneficiaireManquant,
            AnomalyKind.NomOrganisationManquant,
            AnomalyKind.Nom2eBeneficiaireManquant,
            AnomalyKind.Prenom2eBeneficiaireManquant,
            AnomalyKind.CodeDeviseErrone
        }
    };

    public static readonly AnomalyLevelConfig DefaultImportante = new()
    {
        Nombre = 5,
        Types = new[] {
            AnomalyKind.NASManquant,
            AnomalyKind.NEManquant,
            AnomalyKind.NEQManquant,
            AnomalyKind.FIDManquant,
            AnomalyKind.NIManquant
        }
    };

    public static readonly AnomalyLevelConfig DefaultSevereImpression = new()
    {
        Nombre = 5,
        Types = new[] {
            AnomalyKind.CodePostalManquant,
            AnomalyKind.CodeProvinceManquant,
            AnomalyKind.AdresseManquante,
            AnomalyKind.VilleManquante,
            AnomalyKind.CodePaysManquant
        }
    };

    public static readonly AnomalyLevelConfig DefaultAvertissement = new()
    {
        Nombre = 1,
        Types = new[] {
            AnomalyKind.CodeLangueManquant
        }
    };
}



public sealed class AnomalyLevelConfig
{
    public int Nombre { get; set; }
    public AnomalyKind[] Types { get; set; } = [];
}
