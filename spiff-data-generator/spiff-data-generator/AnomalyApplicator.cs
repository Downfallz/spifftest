namespace spiff_data_generator;

public static class AnomalyType
{
    // Bloquant
    public const string NomBeneficiaireManquant = "NomBeneficiaireManquant";
    public const string PrenomBeneficiaireManquant = "PrenomBeneficiaireManquant";
    public const string NomOrganisationManquant = "NomOrganisationManquant";
    public const string Nom2eBeneficiaireManquant = "Nom2eBeneficiaireManquant";
    public const string Prenom2eBeneficiaireManquant = "Prenom2eBeneficiaireManquant";
    public const string CodeDeviseErrone = "CodeDeviseErrone";
    public const string Case13Manquant = "Case13Manquant";

    // Importante (identifiants remplacés par des 0)
    public const string NASManquant = "NASManquant";
    public const string NEManquant = "NEManquant";
    public const string NEQManquant = "NEQManquant";
    public const string FIDManquant = "FIDManquant";
    public const string NIManquant = "NIManquant";

    // Sévère impression (adresse)
    public const string CodePostalManquant = "CodePostalManquant";
    public const string CodeProvinceManquant = "CodeProvinceManquant";
    public const string AdresseManquante = "AdresseManquante";
    public const string VilleManquante = "VilleManquante";
    public const string CodePaysManquant = "CodePaysManquant";

    // Avertissement
    public const string CodeLangueManquant = "CodeLangueManquant";
}

public static class AnomalyApplicator
{
    public static void Apply(Dictionary<string, object> root, string anomalyType, bool isIndividu)
    {
        var info = GetDict(root, "information");
        var parties = GetList(info, "parties");
        var party = parties.Count > 0 ? parties[0] as Dictionary<string, object> : null;
        var adresse = party != null ? GetDictOrNull(party, "adresseFiscale") : null;

        switch (anomalyType)
        {
            // --- Bloquant ---
            case AnomalyType.NomBeneficiaireManquant:
                if (isIndividu && party != null)
                    party["nomFamille"] = "";
                break;

            case AnomalyType.PrenomBeneficiaireManquant:
                if (isIndividu && party != null)
                    party["prn"] = "";
                break;

            case AnomalyType.NomOrganisationManquant:
                if (!isIndividu && party != null)
                    party["nomOrganisationLign1"] = "";
                break;

            case AnomalyType.Nom2eBeneficiaireManquant:
                if (!isIndividu && party != null)
                    party["nomOrganisationLign2"] = "";
                break;

            case AnomalyType.Prenom2eBeneficiaireManquant:
                if (isIndividu && party != null)
                    party["nomInitiale"] = "";
                break;

            case AnomalyType.CodeDeviseErrone:
                info["codDevise"] = "NON";
                break;

            case AnomalyType.Case13Manquant:
                ClearCase(root, "13");
                break;

            // --- Importante (identifiants → 0) ---
            case AnomalyType.NASManquant:
                if (isIndividu && party != null)
                    SetIdentificationToZeros(party, idCodType: 1, zeros: "000000000");
                break;

            case AnomalyType.NEManquant:
                if (!isIndividu && party != null)
                    SetIdentificationToZeros(party, idCodType: 2, zeros: "000000000");
                break;

            case AnomalyType.NEQManquant:
                if (!isIndividu && party != null)
                    SetIdentificationToZeros(party, idCodType: 6, zeros: "0000000000");
                break;

            case AnomalyType.FIDManquant:
                if (!isIndividu && party != null)
                    SetIdentificationToZeros(party, idCodType: 8, zeros: "T00000000");
                break;

            case AnomalyType.NIManquant:
                if (!isIndividu && party != null)
                    SetIdentificationToZeros(party, idCodType: 7, zeros: "0000000000");
                break;

            // --- Sévère impression (adresse) ---
            case AnomalyType.CodePostalManquant:
                if (adresse != null) adresse["numCodePostal"] = "";
                if (adresse != null && adresse.ContainsKey("numCodPostal")) adresse["numCodPostal"] = "";
                break;

            case AnomalyType.CodeProvinceManquant:
                if (adresse != null) adresse["codProvince"] = "";
                break;

            case AnomalyType.AdresseManquante:
                if (adresse != null)
                {
                    adresse["nomRue"] = "";
                    if (adresse.ContainsKey("numCivique")) adresse["numCivique"] = "";
                    if (adresse.ContainsKey("numUnite")) adresse["numUnite"] = "";
                }
                break;

            case AnomalyType.VilleManquante:
                if (adresse != null) adresse["nomMunicipalite"] = "";
                break;

            case AnomalyType.CodePaysManquant:
                if (adresse != null)
                {
                    if (adresse.ContainsKey("codePaysIso")) adresse["codePaysIso"] = "";
                    if (adresse.ContainsKey("codPaysIso")) adresse["codPaysIso"] = "";
                }
                break;

            // --- Avertissement ---
            case AnomalyType.CodeLangueManquant:
                info["codLangue"] = "";
                break;
        }
    }

    private static void SetIdentificationToZeros(Dictionary<string, object> party, int idCodType, string zeros)
    {
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
        if (contenu == null) return;

        var cases = GetListOrNull(contenu, "cases");
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

    private static Dictionary<string, object> GetDict(Dictionary<string, object> parent, string key)
        => (Dictionary<string, object>)parent[key];

    private static Dictionary<string, object>? GetDictOrNull(Dictionary<string, object> parent, string key)
        => parent.TryGetValue(key, out var val) ? val as Dictionary<string, object> : null;

    private static List<object> GetList(Dictionary<string, object> parent, string key)
        => (List<object>)parent[key];

    private static List<object>? GetListOrNull(Dictionary<string, object> parent, string key)
        => parent.TryGetValue(key, out var val) ? val as List<object> : null;
}
