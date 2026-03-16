namespace spiff_data_generator.Common.Constraints;

/// <summary>
/// Poids configurables pour chaque dimension de contrainte.
/// Les poids déterminent l'impact relatif de chaque dimension sur le score total.
/// Le score total est normalisé sur 100.
/// </summary>
public sealed class ConstraintConfig
{
    /// <summary>Activer le calcul de contrainte dans les feuillets. Default: false</summary>
    public bool Enabled { get; set; }

    /// <summary>Poids pour la dimension réglementaire provinciale (QC vs autre). Default: 25</summary>
    public int PoidsReglementaire { get; set; } = 25;

    /// <summary>Poids pour le type de bénéficiaire (individu vs organisation). Default: 15</summary>
    public int PoidsTypeBeneficiaire { get; set; } = 15;

    /// <summary>Poids pour la complexité d'identification (fiducie, multi-ID). Default: 20</summary>
    public int PoidsComplexiteIdentification { get; set; } = 20;

    /// <summary>Poids pour la devise étrangère. Default: 15</summary>
    public int PoidsDeviseEtrangere { get; set; } = 15;

    /// <summary>Poids pour la contrainte d'impression (PN vs N). Default: 15</summary>
    public int PoidsContrainteImpression { get; set; } = 15;

    /// <summary>Poids pour le courrier retenu. Default: 10</summary>
    public int PoidsCourrierRetenu { get; set; } = 10;

    /// <summary>Seuil pour niveau Modéré (inclusive). Default: 26</summary>
    public int SeuilModere { get; set; } = 26;

    /// <summary>Seuil pour niveau Élevé (inclusive). Default: 51</summary>
    public int SeuilEleve { get; set; } = 51;

    /// <summary>Seuil pour niveau Critique (inclusive). Default: 76</summary>
    public int SeuilCritique { get; set; } = 76;
}
