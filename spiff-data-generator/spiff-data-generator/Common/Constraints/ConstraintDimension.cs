namespace spiff_data_generator.Common.Constraints;

/// <summary>
/// Dimensions de contrainte qui affectent la capacité organisationnelle
/// à traiter un feuillet fiscal. Chaque dimension représente un axe
/// de complexité indépendant.
/// </summary>
public enum ConstraintDimension
{
    /// <summary>
    /// Province QC = double déclaration (T5 + RL-3), exigences bilingues,
    /// identifiants supplémentaires (NEQ, NI).
    /// </summary>
    ReglementaireProvincial,

    /// <summary>
    /// Organisation vs Individu. Les organisations ont des règles d'identification
    /// plus complexes (NE, NEQ, FID, NI selon le genre).
    /// </summary>
    TypeBeneficiaire,

    /// <summary>
    /// Fiducie = sous-type le plus contraignant (NEQ+NI en QC, FID hors QC,
    /// métadonnées PDO supplémentaires).
    /// </summary>
    ComplexiteIdentification,

    /// <summary>
    /// Devise étrangère = validation supplémentaire, conversion, case 13 obligatoire.
    /// </summary>
    DeviseEtrangere,

    /// <summary>
    /// Impression papier (PN) = contrainte logistique d'impression et d'envoi postal.
    /// </summary>
    ContrainteImpression,

    /// <summary>
    /// Courrier retenu = processus de rétention, gestion des exceptions.
    /// </summary>
    CourrierRetenu,
}
