using spiff_data_generator.T5Rl3.Config;

namespace spiff_data_generator.Common.Anomalies;

public sealed class AnomalyService : IAnomalyService
{
    private readonly T5Rl3Config _config;

    public AnomalyService(T5Rl3Config config)
    {
        _config = config;
    }

    public (AnomalyKind Kind, AnomalySeverity Severity)? GetAnomalyForSequence(int seq)
    {
        if (!_config.Anomalies.Enabled) return null;

        int totalAnomalies = TotalAnomalyCount();
        int anomalyStart = _config.NombreLignes - totalAnomalies + 1;
        if (seq < anomalyStart) return null;

        int offset = seq - anomalyStart;
        foreach (var (level, severity) in GetLevelsWithSeverity())
        {
            if (level.Nombre <= 0 || level.Types.Length == 0) continue;

            if (offset < level.Nombre)
                return (level.Types[offset % level.Types.Length], severity);

            offset -= level.Nombre;
        }

        return null;
    }

    public void Apply(Dictionary<string, object> root, AnomalyKind kind, bool isIndividu)
    {
        var info = GetDict(root, "information");
        var parties = GetList(info, "parties");
        var party = parties.Count > 0 ? parties[0] as Dictionary<string, object> : null;
        var adresse = party != null ? GetDictOrNull(party, "adresseFiscale") : null;

        switch (kind)
        {
            // ── Bloquant ────────────────────────────────────────
            case AnomalyKind.NomBeneficiaireManquant:
                if (isIndividu && party != null) party["nomFamille"] = "";
                break;

            case AnomalyKind.PrenomBeneficiaireManquant:
                if (isIndividu && party != null) party["prn"] = "";
                break;

            case AnomalyKind.NomOrganisationManquant:
                if (!isIndividu && party != null) party["nomOrganisationLign1"] = "";
                break;

            case AnomalyKind.Nom2eBeneficiaireManquant:
                if (!isIndividu && party != null) party["nomOrganisationLign2"] = "";
                break;

            case AnomalyKind.Prenom2eBeneficiaireManquant:
                if (isIndividu && party != null) party["nomInitiale"] = "";
                break;

            case AnomalyKind.CodeDeviseErrone:
                info["codDevise"] = "NON";
                break;

            case AnomalyKind.Case13Manquant:
                ClearCase(root, "13");
                break;

            // ── Importante (identifiants → zéros) ───────────────
            case AnomalyKind.NASManquant:
                if (isIndividu) SetIdentificationToZeros(party, 1, "000000000");
                break;

            case AnomalyKind.NEManquant:
                if (!isIndividu) SetIdentificationToZeros(party, 2, "000000000");
                break;

            case AnomalyKind.NEQManquant:
                if (!isIndividu) SetIdentificationToZeros(party, 6, "0000000000");
                break;

            case AnomalyKind.FIDManquant:
                if (!isIndividu) SetIdentificationToZeros(party, 8, "T00000000");
                break;

            case AnomalyKind.NIManquant:
                if (!isIndividu) SetIdentificationToZeros(party, 7, "0000000000");
                break;

            // ── Sévère impression (adresse) ─────────────────────
            case AnomalyKind.CodePostalManquant:
                ClearAdresseField(adresse, "numCodePostal");
                ClearAdresseField(adresse, "numCodPostal");
                break;

            case AnomalyKind.CodeProvinceManquant:
                ClearAdresseField(adresse, "codProvince");
                break;

            case AnomalyKind.AdresseManquante:
                if (adresse != null)
                {
                    ClearAdresseField(adresse, "nomRue");
                    ClearAdresseField(adresse, "numCivique");
                    ClearAdresseField(adresse, "numUnite");
                }
                break;

            case AnomalyKind.VilleManquante:
                ClearAdresseField(adresse, "nomMunicipalite");
                break;

            case AnomalyKind.CodePaysManquant:
                ClearAdresseField(adresse, "codePaysIso");
                ClearAdresseField(adresse, "codPaysIso");
                break;

            // ── Avertissement ───────────────────────────────────
            case AnomalyKind.CodeLangueManquant:
                info["codLangue"] = "";
                break;
        }
    }

    // ── Private helpers ─────────────────────────────────────────

    private int TotalAnomalyCount() =>
        GetLevelsWithSeverity().Sum(l => l.Level.Nombre);

    private (AnomalyLevelConfig Level, AnomalySeverity Severity)[] GetLevelsWithSeverity() =>
    [
        (_config.Anomalies.Bloquant, AnomalySeverity.Bloquant),
        (_config.Anomalies.Importante, AnomalySeverity.Importante),
        (_config.Anomalies.SevereImpression, AnomalySeverity.SevereImpression),
        (_config.Anomalies.Avertissement, AnomalySeverity.Avertissement),
    ];

    private static void SetIdentificationToZeros(Dictionary<string, object>? party, int idCodType, string zeros)
    {
        if (party == null) return;
        var identList = GetListOrNull(party, "identificationPartie");
        if (identList == null) return;

        foreach (var item in identList)
        {
            if (item is Dictionary<string, object> ident
                && ident.TryGetValue("idCodTypeIdentificationPartie", out var val)
                && Convert.ToInt32(val) == idCodType)
            {
                ident["numIdentificationPartie"] = zeros;
                return;
            }
        }
    }

    private static void ClearCase(Dictionary<string, object> root, string caseNum)
    {
        var contenu = GetDictOrNull(root, "contenu");
        var cases = contenu != null ? GetListOrNull(contenu, "cases") : null;
        if (cases == null) return;

        foreach (var item in cases)
        {
            if (item is Dictionary<string, object> c
                && c.TryGetValue("case", out var val)
                && val?.ToString() == caseNum)
            {
                c["valeur"] = "";
                return;
            }
        }
    }

    private static void ClearAdresseField(Dictionary<string, object>? adresse, string key)
    {
        if (adresse != null && adresse.ContainsKey(key))
            adresse[key] = "";
    }

    private static Dictionary<string, object> GetDict(Dictionary<string, object> parent, string key)
        => (Dictionary<string, object>)parent[key];

    private static Dictionary<string, object>? GetDictOrNull(Dictionary<string, object> parent, string key)
        => parent.TryGetValue(key, out var val) ? val as Dictionary<string, object> : null;

    private static List<object> GetList(Dictionary<string, object> parent, string key)
        => (List<object>)parent[key];

    private static List<object>? GetListOrNull(Dictionary<string, object> parent, string key)
        => parent.TryGetValue(key, out var val) ? val as List<object> : null;
}
