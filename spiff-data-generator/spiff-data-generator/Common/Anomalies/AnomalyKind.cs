namespace spiff_data_generator.Common.Anomalies;

public enum AnomalyKind
{
    // Bloquant
    NomBeneficiaireManquant,
    PrenomBeneficiaireManquant,
    NomOrganisationManquant,
    Nom2eBeneficiaireManquant,
    Prenom2eBeneficiaireManquant,
    CodeDeviseErrone,
    Case13Manquant,

    // Importante
    NASManquant,
    NEManquant,
    NEQManquant,
    FIDManquant,
    NIManquant,

    // Sévère impression
    CodePostalManquant,
    CodeProvinceManquant,
    AdresseManquante,
    VilleManquante,
    CodePaysManquant,

    // Avertissement
    CodeLangueManquant,
}
