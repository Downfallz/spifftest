using FluentAssertions;
using spiff_data_generator;
using Xunit;

namespace spiff_data_generator.Tests;

public class AnomalyApplicatorTests
{
    // ========== Helpers ==========

    private static Dictionary<string, object> BuildIndividuRoot(
        string nom = "TREMBLAY",
        string prenom = "JEAN",
        string initiale = "J",
        string codDevise = "CAD",
        string codLangue = "F",
        string codProvince = "QC",
        string numCodePostal = "G1K2A3",
        string codePaysIso = "CAN",
        string nomRue = "RUE PRINCIPALE",
        string numCivique = "123",
        string nomMunicipalite = "QUEBEC",
        string nas = "123456789",
        string case13 = "1000.00")
    {
        return new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codFormulaire"] = "T5RL3",
                ["codLangue"] = codLangue,
                ["codDevise"] = codDevise,
                ["typImpression"] = "PN",
                ["holdMailIndicateur"] = false,
                ["numIdentification"] = "81500008",
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["isCodSousTypePartie"] = 1,
                        ["idCodRoleRelevePartie"] = 1,
                        ["idCodTypeRoleRelevePartie"] = 1,
                        ["identificationPartie"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 1,
                                ["numIdentificationPartie"] = nas
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 4,
                                ["numIdentificationPartie"] = "81500008000001"
                            }
                        },
                        ["prn"] = prenom,
                        ["nomFamille"] = nom,
                        ["nomInitiale"] = initiale,
                        ["adresseFiscale"] = new Dictionary<string, object>
                        {
                            ["numCivique"] = numCivique,
                            ["nomRue"] = nomRue,
                            ["nomMunicipalite"] = nomMunicipalite,
                            ["numUnite"] = "APT 2",
                            ["codProvince"] = codProvince,
                            ["numCodePostal"] = numCodePostal,
                            ["codePaysIso"] = codePaysIso,
                        },
                        ["indAdrFiscalePostaleIdentique"] = true,
                    }
                }
            },
            ["documents"] = new List<object>(),
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = new List<object>
                {
                    new Dictionary<string, object> { ["case"] = "13", ["valeur"] = case13 },
                    new Dictionary<string, object> { ["case"] = "2B", ["valeur"] = "81500008" },
                }
            }
        };
    }

    private static Dictionary<string, object> BuildOrganisationRoot(
        string nomOrg1 = "DESJARDINS INC",
        string nomOrg2 = "FILIALE",
        string codDevise = "CAD",
        string codLangue = "A",
        string codProvince = "ON",
        string numCodPostal = "K1A0B1",
        string codPaysIso = "CAN",
        string ne = "123456789",
        string neq = "3312345678",
        string fid = "T12345678",
        string ni = "1234567890")
    {
        return new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codFormulaireReleve"] = "T5",
                ["codLangue"] = codLangue,
                ["codDevise"] = codDevise,
                ["typImpression"] = "N",
                ["holdMail"] = false,
                ["numIdentificationEmetteur"] = "32900303",
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["idCodSousTypePartie"] = 2,
                        ["idCodRoleRelevePartie"] = 1,
                        ["idCodTypeRoleRelevePartie"] = 3,
                        ["identificationPartie"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 4,
                                ["numIdentificationPartie"] = "32900303000001"
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 2, // NE
                                ["numIdentificationPartie"] = ne
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 6, // NEQ
                                ["numIdentificationPartie"] = neq
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 8, // FID
                                ["numIdentificationPartie"] = fid
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 7, // NI
                                ["numIdentificationPartie"] = ni
                            }
                        },
                        ["nomOrganisationLign1"] = nomOrg1,
                        ["nomOrganisationLign2"] = nomOrg2,
                        ["adresseFiscale"] = new Dictionary<string, object>
                        {
                            ["numCivique"] = "500",
                            ["nomRue"] = "MAIN STREET",
                            ["nomMunicipalite"] = "OTTAWA",
                            ["numUnite"] = "SUITE 100",
                            ["codProvince"] = codProvince,
                            ["codPaysIso"] = codPaysIso,
                            ["numCodPostal"] = numCodPostal,
                        },
                        ["indicateurAdresseFiscalePostalIdentique"] = true,
                    }
                },
                ["documents"] = new List<object>()
            },
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = new List<object>
                {
                    new Dictionary<string, object> { ["case"] = "13", ["valeur"] = "5000.00" },
                    new Dictionary<string, object> { ["case"] = "2B", ["valeur"] = "32900303" },
                }
            }
        };
    }

    private static Dictionary<string, object> GetParty(Dictionary<string, object> root)
    {
        var info = (Dictionary<string, object>)root["information"];
        var parties = (List<object>)info["parties"];
        return (Dictionary<string, object>)parties[0];
    }

    private static Dictionary<string, object> GetAdresse(Dictionary<string, object> root)
    {
        var party = GetParty(root);
        return (Dictionary<string, object>)party["adresseFiscale"];
    }

    private static string GetIdentificationValue(Dictionary<string, object> root, int idCodType)
    {
        var party = GetParty(root);
        var idents = (List<object>)party["identificationPartie"];
        foreach (var item in idents)
        {
            var dict = (Dictionary<string, object>)item;
            if (Convert.ToInt32(dict["idCodTypeIdentificationPartie"]) == idCodType)
                return dict["numIdentificationPartie"].ToString()!;
        }
        return "";
    }

    private static string GetCaseValue(Dictionary<string, object> root, string caseNum)
    {
        var contenu = (Dictionary<string, object>)root["contenu"];
        var cases = (List<object>)contenu["cases"];
        foreach (var item in cases)
        {
            var dict = (Dictionary<string, object>)item;
            if (dict["case"].ToString() == caseNum)
                return dict["valeur"].ToString()!;
        }
        return "";
    }

    // ========== Bloquant Tests ==========

    [Fact]
    public void NomBeneficiaireManquant_ShouldClearNomFamille_WhenIndividu()
    {
        var root = BuildIndividuRoot(nom: "TREMBLAY");

        AnomalyApplicator.Apply(root, AnomalyType.NomBeneficiaireManquant, isIndividu: true);

        GetParty(root)["nomFamille"].Should().Be("");
    }

    [Fact]
    public void NomBeneficiaireManquant_ShouldNotAffect_WhenOrganisation()
    {
        var root = BuildOrganisationRoot();

        AnomalyApplicator.Apply(root, AnomalyType.NomBeneficiaireManquant, isIndividu: false);

        GetParty(root)["nomOrganisationLign1"].Should().Be("DESJARDINS INC");
    }

    [Fact]
    public void PrenomBeneficiaireManquant_ShouldClearPrenom()
    {
        var root = BuildIndividuRoot(prenom: "JEAN");

        AnomalyApplicator.Apply(root, AnomalyType.PrenomBeneficiaireManquant, isIndividu: true);

        GetParty(root)["prn"].Should().Be("");
    }

    [Fact]
    public void NomOrganisationManquant_ShouldClearNomOrg_WhenOrganisation()
    {
        var root = BuildOrganisationRoot(nomOrg1: "DESJARDINS INC");

        AnomalyApplicator.Apply(root, AnomalyType.NomOrganisationManquant, isIndividu: false);

        GetParty(root)["nomOrganisationLign1"].Should().Be("");
    }

    [Fact]
    public void NomOrganisationManquant_ShouldNotAffect_WhenIndividu()
    {
        var root = BuildIndividuRoot(nom: "TREMBLAY");

        AnomalyApplicator.Apply(root, AnomalyType.NomOrganisationManquant, isIndividu: true);

        GetParty(root)["nomFamille"].Should().Be("TREMBLAY");
    }

    [Fact]
    public void Nom2eBeneficiaireManquant_ShouldClearNomOrgLign2()
    {
        var root = BuildOrganisationRoot(nomOrg2: "FILIALE");

        AnomalyApplicator.Apply(root, AnomalyType.Nom2eBeneficiaireManquant, isIndividu: false);

        GetParty(root)["nomOrganisationLign2"].Should().Be("");
    }

    [Fact]
    public void Prenom2eBeneficiaireManquant_ShouldClearInitiale()
    {
        var root = BuildIndividuRoot(initiale: "J");

        AnomalyApplicator.Apply(root, AnomalyType.Prenom2eBeneficiaireManquant, isIndividu: true);

        GetParty(root)["nomInitiale"].Should().Be("");
    }

    [Fact]
    public void CodeDeviseErrone_ShouldSetToNON()
    {
        var root = BuildIndividuRoot(codDevise: "CAD");

        AnomalyApplicator.Apply(root, AnomalyType.CodeDeviseErrone, isIndividu: true);

        var info = (Dictionary<string, object>)root["information"];
        info["codDevise"].Should().Be("NON");
    }

    [Fact]
    public void Case13Manquant_ShouldClearCase13Value()
    {
        var root = BuildIndividuRoot(case13: "1234.56");

        AnomalyApplicator.Apply(root, AnomalyType.Case13Manquant, isIndividu: true);

        GetCaseValue(root, "13").Should().Be("");
    }

    // ========== Importante Tests ==========

    [Fact]
    public void NASManquant_ShouldReplaceWithZeros()
    {
        var root = BuildIndividuRoot(nas: "123456789");

        AnomalyApplicator.Apply(root, AnomalyType.NASManquant, isIndividu: true);

        GetIdentificationValue(root, idCodType: 1).Should().Be("000000000");
    }

    [Fact]
    public void NEManquant_ShouldReplaceWithZeros()
    {
        var root = BuildOrganisationRoot(ne: "999888777");

        AnomalyApplicator.Apply(root, AnomalyType.NEManquant, isIndividu: false);

        GetIdentificationValue(root, idCodType: 2).Should().Be("000000000");
    }

    [Fact]
    public void NEQManquant_ShouldReplaceWithZeros()
    {
        var root = BuildOrganisationRoot(neq: "3312345678");

        AnomalyApplicator.Apply(root, AnomalyType.NEQManquant, isIndividu: false);

        GetIdentificationValue(root, idCodType: 6).Should().Be("0000000000");
    }

    [Fact]
    public void FIDManquant_ShouldReplaceWithZeros()
    {
        var root = BuildOrganisationRoot(fid: "T12345678");

        AnomalyApplicator.Apply(root, AnomalyType.FIDManquant, isIndividu: false);

        GetIdentificationValue(root, idCodType: 8).Should().Be("T00000000");
    }

    [Fact]
    public void NIManquant_ShouldReplaceWithZeros()
    {
        var root = BuildOrganisationRoot(ni: "1234567890");

        AnomalyApplicator.Apply(root, AnomalyType.NIManquant, isIndividu: false);

        GetIdentificationValue(root, idCodType: 7).Should().Be("0000000000");
    }

    [Fact]
    public void NASManquant_ShouldNotAffect_WhenOrganisation()
    {
        var root = BuildOrganisationRoot();

        AnomalyApplicator.Apply(root, AnomalyType.NASManquant, isIndividu: false);

        // NAS (type 1) n'existe pas dans org, pas d'exception
        GetIdentificationValue(root, idCodType: 4).Should().NotBeEmpty();
    }

    // ========== Sévère impression Tests ==========

    [Fact]
    public void CodePostalManquant_ShouldClearPostalCode_Individu()
    {
        var root = BuildIndividuRoot(numCodePostal: "G1K2A3");

        AnomalyApplicator.Apply(root, AnomalyType.CodePostalManquant, isIndividu: true);

        GetAdresse(root)["numCodePostal"].Should().Be("");
    }

    [Fact]
    public void CodePostalManquant_ShouldClearPostalCode_Organisation()
    {
        var root = BuildOrganisationRoot(numCodPostal: "K1A0B1");

        AnomalyApplicator.Apply(root, AnomalyType.CodePostalManquant, isIndividu: false);

        GetAdresse(root)["numCodPostal"].Should().Be("");
    }

    [Fact]
    public void CodeProvinceManquant_ShouldClearProvince()
    {
        var root = BuildIndividuRoot(codProvince: "QC");

        AnomalyApplicator.Apply(root, AnomalyType.CodeProvinceManquant, isIndividu: true);

        GetAdresse(root)["codProvince"].Should().Be("");
    }

    [Fact]
    public void AdresseManquante_ShouldClearAddressFields()
    {
        var root = BuildIndividuRoot(nomRue: "RUE PRINCIPALE", numCivique: "123");

        AnomalyApplicator.Apply(root, AnomalyType.AdresseManquante, isIndividu: true);

        var adresse = GetAdresse(root);
        adresse["nomRue"].Should().Be("");
        adresse["numCivique"].Should().Be("");
        adresse["numUnite"].Should().Be("");
    }

    [Fact]
    public void VilleManquante_ShouldClearMunicipalite()
    {
        var root = BuildIndividuRoot(nomMunicipalite: "QUEBEC");

        AnomalyApplicator.Apply(root, AnomalyType.VilleManquante, isIndividu: true);

        GetAdresse(root)["nomMunicipalite"].Should().Be("");
    }

    [Fact]
    public void CodePaysManquant_ShouldClearPays_Individu()
    {
        var root = BuildIndividuRoot(codePaysIso: "CAN");

        AnomalyApplicator.Apply(root, AnomalyType.CodePaysManquant, isIndividu: true);

        GetAdresse(root)["codePaysIso"].Should().Be("");
    }

    [Fact]
    public void CodePaysManquant_ShouldClearPays_Organisation()
    {
        var root = BuildOrganisationRoot(codPaysIso: "CAN");

        AnomalyApplicator.Apply(root, AnomalyType.CodePaysManquant, isIndividu: false);

        GetAdresse(root)["codPaysIso"].Should().Be("");
    }

    // ========== Avertissement Tests ==========

    [Fact]
    public void CodeLangueManquant_ShouldClearLangue()
    {
        var root = BuildIndividuRoot(codLangue: "F");

        AnomalyApplicator.Apply(root, AnomalyType.CodeLangueManquant, isIndividu: true);

        var info = (Dictionary<string, object>)root["information"];
        info["codLangue"].Should().Be("");
    }

    // ========== Edge Cases ==========

    [Fact]
    public void Apply_ShouldNotThrow_WhenIdentificationTypeNotFound()
    {
        var root = BuildIndividuRoot();

        // FID (type 8) n'existe pas dans un individu
        var act = () => AnomalyApplicator.Apply(root, AnomalyType.FIDManquant, isIndividu: false);

        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_MultipleAnomalies_ShouldStackCorrectly()
    {
        var root = BuildIndividuRoot(nom: "TREMBLAY", codDevise: "CAD");

        AnomalyApplicator.Apply(root, AnomalyType.NomBeneficiaireManquant, isIndividu: true);
        AnomalyApplicator.Apply(root, AnomalyType.CodeDeviseErrone, isIndividu: true);

        GetParty(root)["nomFamille"].Should().Be("");
        var info = (Dictionary<string, object>)root["information"];
        info["codDevise"].Should().Be("NON");
    }
}
