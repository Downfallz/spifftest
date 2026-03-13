using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.T5Rl3.Config;

namespace spiff_data_generator.Api;

/// <summary>
/// Paramètres de génération T5RL3/T5. Tous les champs ont des valeurs par défaut;
/// un POST avec body vide {} génère 20 lignes (10 individus + 10 organisations).
/// </summary>
public sealed class GenerateRequest
{
    /// <summary>Nombre de feuillets de type Individu (les premiers N feuillets). Default: 10</summary>
    [Range(1, 10_000_000)]
    [DefaultValue(10)]
    public int NombreIndividus { get; set; } = 10;

    /// <summary>Nombre total de feuillets (individus + organisations). Default: 20</summary>
    [Range(1, 10_000_000)]
    [DefaultValue(20)]
    public int NombreLignes { get; set; } = 20;

    /// <summary>Nombre de feuillets par fichier JSON dans le ZIP. Default: 1000</summary>
    [Range(1, 100_000)]
    [DefaultValue(1000)]
    public int BatchSize { get; set; } = 1000;

    /// <summary>Seed pour le générateur aléatoire. 0 = aléatoire. Default: 0</summary>
    [DefaultValue(0)]
    public int Seed { get; set; }

    /// <summary>Code plateforme. Default: "SPIFF"</summary>
    [DefaultValue("SPIFF")]
    public string Plateforme { get; set; } = "SPIFF";

    /// <summary>Code système. Default: "D10815"</summary>
    [DefaultValue("D10815")]
    public string CodeSysteme { get; set; } = "D10815";

    /// <summary>Type de déclaration (O=Originale, A=Amendée). Default: "O"</summary>
    [DefaultValue("O")]
    public string TypeDeclaration { get; set; } = "O";

    /// <summary>Cycle de production (A=Annuel). Default: "A"</summary>
    [DefaultValue("A")]
    public string CycleProduction { get; set; } = "A";

    /// <summary>Année de production. Default: "2026"</summary>
    [DefaultValue("2026")]
    public string AnneeProduction { get; set; } = "2026";

    /// <summary>
    /// Poids [courrierRetenu, nonRetenu] pour WeightedChoice.
    /// Ex: [5, 95] = 5% retenu, 95% non retenu. Doit avoir exactement 2 éléments.
    /// </summary>
    public int[] WeightsCourrierRetenu { get; set; } = [5, 95];

    /// <summary>
    /// Poids [PN, N] pour le type d'impression.
    /// Ex: [80, 20] = 80% impression PN, 20% impression N. Doit avoir exactement 2 éléments.
    /// </summary>
    public int[] WeightsImpression { get; set; } = [80, 20];

    /// <summary>
    /// Poids [QC, Autre] pour la province.
    /// Ex: [70, 30] = 70% Québec, 30% autre province. Doit avoir exactement 2 éléments.
    /// </summary>
    public int[] WeightsCodeProvince { get; set; } = [70, 30];

    /// <summary>
    /// Si true, toutes les adresses hors-QC sont ON (Ontario).
    /// Si false, province hors-QC aléatoire. Default: false
    /// </summary>
    [DefaultValue(false)]
    public bool IndicateurOntario { get; set; }

    /// <summary>Nombre de feuillets par caisse (transit). Contrôle la rotation des numéros de transit. Default: 999999</summary>
    [DefaultValue(999_999)]
    public int NombreFeuilletParCaisse { get; set; } = 999_999;

    /// <summary>
    /// Ajouter l'émetteur fourni (transit) comme identification supplémentaire (type 5) dans chaque feuillet.
    /// Default: false
    /// </summary>
    [DefaultValue(false)]
    public bool AjouterEmetteurFourni { get; set; }

    /// <summary>
    /// Ajouter un identifiant unique par feuillet (numIdentificationUnique).
    /// Nécessaire si la règle RS-IMPORT-CORRECTIF est configurée. Default: false
    /// </summary>
    [DefaultValue(false)]
    public bool AjouterIdUnique { get; set; }

    /// <summary>Préfixe pour l'identifiant unique. Ex: "ADO-2026-". Default: ""</summary>
    [DefaultValue("")]
    public string PrefixeIdentificationUnique { get; set; } = "";

    /// <summary>
    /// Liste des devises possibles (ISO 4217).
    /// Default: ["CAD","USD","AUD","DKK","EUR","GBP","HKD","JPY","NZD"]
    /// </summary>
    public string[] Devises { get; set; } =
        ["CAD", "USD", "AUD", "DKK", "EUR", "GBP", "HKD", "JPY", "NZD"];

    /// <summary>JSON indenté (lisible) ou compact. Default: false</summary>
    [DefaultValue(false)]
    public bool PrettyPrint { get; set; }

    /// <summary>
    /// Configuration des anomalies à injecter. Si null ou Enabled=false, aucune anomalie.
    /// Les anomalies sont appliquées aux derniers feuillets de la séquence.
    /// </summary>
    public AnomalyConfig? Anomalies { get; set; }

    /// <summary>Chemin FTP (mock = disque local). Default: "out/ftp"</summary>
    [DefaultValue("out/ftp")]
    public string FtpPath { get; set; } = "out/ftp";

    /// <summary>Nombre de tentatives d'upload FTP. Default: 3</summary>
    [Range(1, 10)]
    [DefaultValue(3)]
    public int FtpRetryCount { get; set; } = 3;

    /// <summary>Délai entre les tentatives FTP (secondes). Default: 5</summary>
    [Range(1, 60)]
    [DefaultValue(5)]
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
