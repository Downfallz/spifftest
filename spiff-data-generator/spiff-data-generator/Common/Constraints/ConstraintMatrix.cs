using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.Common.Constraints;

/// <summary>
/// Matrice de contrainte × capacité organisationnelle.
///
/// Calcule un score de contrainte [0-100] pour chaque feuillet en croisant
/// ses caractéristiques (province, type, devise, etc.) avec les poids
/// configurés par dimension. Le score reflète l'effort organisationnel
/// requis pour traiter ce feuillet.
///
/// Matrice conceptuelle:
/// ┌──────────────────────┬───────────┬───────────┐
/// │ Dimension            │ Faible    │ Élevé     │
/// ├──────────────────────┼───────────┼───────────┤
/// │ Réglementaire        │ Hors-QC   │ QC (RL-3) │
/// │ Type bénéficiaire    │ Individu  │ Org       │
/// │ Complexité ID        │ Simple    │ Fiducie   │
/// │ Devise               │ CAD       │ Étrangère │
/// │ Impression           │ N (élec.) │ PN (pap.) │
/// │ Courrier retenu      │ Non       │ Oui       │
/// └──────────────────────┴───────────┴───────────┘
/// </summary>
public static class ConstraintMatrix
{
    /// <summary>
    /// Évalue le profil de contrainte d'un feuillet à partir de son contexte.
    /// </summary>
    public static ConstraintProfile Evaluate(SlipContext context, ConstraintConfig config)
    {
        var details = new Dictionary<ConstraintDimension, int>();

        // Réglementaire: QC = poids complet, hors-QC = 0
        details[ConstraintDimension.ReglementaireProvincial] =
            context.IsQc ? config.PoidsReglementaire : 0;

        // Type bénéficiaire: Organisation = poids complet, Individu = 0
        details[ConstraintDimension.TypeBeneficiaire] =
            context.IsIndividu ? 0 : config.PoidsTypeBeneficiaire;

        // Complexité identification:
        // Fiducie QC (NEQ+NI) = poids complet
        // Fiducie hors-QC (FID) ou Org QC (NEQ) = moitié
        // Individu ou Org hors-QC = 0
        details[ConstraintDimension.ComplexiteIdentification] =
            EvaluerComplexiteIdentification(context, config.PoidsComplexiteIdentification);

        // Devise étrangère: non-CAD = poids complet, CAD = 0
        details[ConstraintDimension.DeviseEtrangere] =
            context.Devise != "CAD" ? config.PoidsDeviseEtrangere : 0;

        // Impression: PN (papier) = poids complet, N (électronique) = 0
        details[ConstraintDimension.ContrainteImpression] =
            context.TypImpression == "PN" ? config.PoidsContrainteImpression : 0;

        // Courrier retenu = poids complet si retenu
        details[ConstraintDimension.CourrierRetenu] =
            context.HoldMail ? config.PoidsCourrierRetenu : 0;

        int rawScore = details.Values.Sum();
        int maxScore = config.PoidsReglementaire
            + config.PoidsTypeBeneficiaire
            + config.PoidsComplexiteIdentification
            + config.PoidsDeviseEtrangere
            + config.PoidsContrainteImpression
            + config.PoidsCourrierRetenu;

        // Normaliser sur 100
        int score = maxScore > 0 ? (int)Math.Round(rawScore * 100.0 / maxScore) : 0;

        var niveau = score >= config.SeuilCritique ? ConstraintNiveau.Critique
            : score >= config.SeuilEleve ? ConstraintNiveau.Eleve
            : score >= config.SeuilModere ? ConstraintNiveau.Modere
            : ConstraintNiveau.Faible;

        return new ConstraintProfile(score, niveau, details);
    }

    private static int EvaluerComplexiteIdentification(SlipContext context, int poidsMax)
    {
        if (context.IsIndividu)
            return 0;

        // Org QC = complexité moyenne (NEQ requis, possiblement Fiducie QC = NEQ+NI)
        // Org hors-QC = complexité faible (NE ou FID seul)
        // On ne connaît pas le genre d'organisation ici, donc on utilise
        // la province comme proxy: QC = poids complet, hors-QC = moitié
        return context.IsQc ? poidsMax : poidsMax / 2;
    }
}
