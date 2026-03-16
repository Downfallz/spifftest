namespace spiff_data_generator.Common.Constraints;

/// <summary>
/// Profil de contrainte calculé pour un feuillet. Capture le score total
/// et le détail par dimension pour traçabilité.
/// </summary>
/// <param name="Score">Score total de contrainte (0-100). Plus le score est élevé,
/// plus le feuillet est exigeant pour l'organisation.</param>
/// <param name="Niveau">Niveau qualitatif dérivé du score.</param>
/// <param name="Details">Score par dimension individuelle.</param>
public sealed record ConstraintProfile(
    int Score,
    ConstraintNiveau Niveau,
    IReadOnlyDictionary<ConstraintDimension, int> Details);

/// <summary>
/// Niveau qualitatif de contrainte organisationnelle.
/// Les seuils par défaut: Faible [0-25], Modere [26-50], Eleve [51-75], Critique [76-100].
/// </summary>
public enum ConstraintNiveau
{
    Faible,
    Modere,
    Eleve,
    Critique,
}
