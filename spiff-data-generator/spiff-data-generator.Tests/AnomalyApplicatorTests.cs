using FluentAssertions;
using spiff_data_generator;
using Xunit;

namespace spiff_data_generator.Tests;

public class AnomalyServiceTests
{
    private readonly AnomalyService _sut;

    public AnomalyServiceTests()
    {
        var config = new T5Rl3Config { NombreLignes = 100 };
        _sut = new AnomalyService(config);
    }

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
                    }
                }
            },
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
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["idCodSousTypePartie"] = 2,
                        ["identificationPartie"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 4,
                                ["numIdentificationPartie"] = "32900303000001"
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 2,
                                ["numIdentificationPartie"] = ne
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 6,
                                ["numIdentificationPartie"] = neq
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 8,
                                ["numIdentificationPartie"] = fid
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 7,
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
                    }
                },
            },
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = new List<object>
                {
                    new Dictionary<string, object> { ["case"] = "13", ["valeur"] = "5000.00" },
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
        => (Dictionary<string, object>)GetParty(root)["adresseFiscale"];

    private static string GetIdentificationValue(Dictionary<string, object> root, int idCodType)
    {
        var idents = (List<object>)GetParty(root)["identificationPartie"];
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

    // ========== Bloquant ==========

    [Fact]
    public void NomBeneficiaireManquant_ClearsNomFamille()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.NomBeneficiaireManquant, isIndividu: true);
        GetParty(root)["nomFamille"].Should().Be("");
    }

    [Fact]
    public void NomBeneficiaireManquant_DoesNotAffectOrganisation()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.NomBeneficiaireManquant, isIndividu: false);
        GetParty(root)["nomOrganisationLign1"].Should().Be("DESJARDINS INC");
    }

    [Fact]
    public void PrenomBeneficiaireManquant_ClearsPrenom()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.PrenomBeneficiaireManquant, isIndividu: true);
        GetParty(root)["prn"].Should().Be("");
    }

    [Fact]
    public void NomOrganisationManquant_ClearsNomOrg()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.NomOrganisationManquant, isIndividu: false);
        GetParty(root)["nomOrganisationLign1"].Should().Be("");
    }

    [Fact]
    public void NomOrganisationManquant_DoesNotAffectIndividu()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.NomOrganisationManquant, isIndividu: true);
        GetParty(root)["nomFamille"].Should().Be("TREMBLAY");
    }

    [Fact]
    public void Nom2eBeneficiaireManquant_ClearsNomOrgLign2()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.Nom2eBeneficiaireManquant, isIndividu: false);
        GetParty(root)["nomOrganisationLign2"].Should().Be("");
    }

    [Fact]
    public void Prenom2eBeneficiaireManquant_ClearsInitiale()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.Prenom2eBeneficiaireManquant, isIndividu: true);
        GetParty(root)["nomInitiale"].Should().Be("");
    }

    [Fact]
    public void CodeDeviseErrone_SetsToNON()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.CodeDeviseErrone, isIndividu: true);
        ((Dictionary<string, object>)root["information"])["codDevise"].Should().Be("NON");
    }

    [Fact]
    public void Case13Manquant_ClearsCase13()
    {
        var root = BuildIndividuRoot(case13: "1234.56");
        _sut.Apply(root, AnomalyKind.Case13Manquant, isIndividu: true);
        GetCaseValue(root, "13").Should().Be("");
    }

    // ========== Importante ==========

    [Fact]
    public void NASManquant_ReplacesWithZeros()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.NASManquant, isIndividu: true);
        GetIdentificationValue(root, 1).Should().Be("000000000");
    }

    [Fact]
    public void NEManquant_ReplacesWithZeros()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.NEManquant, isIndividu: false);
        GetIdentificationValue(root, 2).Should().Be("000000000");
    }

    [Fact]
    public void NEQManquant_ReplacesWithZeros()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.NEQManquant, isIndividu: false);
        GetIdentificationValue(root, 6).Should().Be("0000000000");
    }

    [Fact]
    public void FIDManquant_ReplacesWithZeros()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.FIDManquant, isIndividu: false);
        GetIdentificationValue(root, 8).Should().Be("T00000000");
    }

    [Fact]
    public void NIManquant_ReplacesWithZeros()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.NIManquant, isIndividu: false);
        GetIdentificationValue(root, 7).Should().Be("0000000000");
    }

    // ========== Sévère impression ==========

    [Fact]
    public void CodePostalManquant_ClearsPostalCode_Individu()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.CodePostalManquant, isIndividu: true);
        GetAdresse(root)["numCodePostal"].Should().Be("");
    }

    [Fact]
    public void CodePostalManquant_ClearsPostalCode_Organisation()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.CodePostalManquant, isIndividu: false);
        GetAdresse(root)["numCodPostal"].Should().Be("");
    }

    [Fact]
    public void CodeProvinceManquant_ClearsProvince()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.CodeProvinceManquant, isIndividu: true);
        GetAdresse(root)["codProvince"].Should().Be("");
    }

    [Fact]
    public void AdresseManquante_ClearsAllAddressFields()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.AdresseManquante, isIndividu: true);
        var adresse = GetAdresse(root);
        adresse["nomRue"].Should().Be("");
        adresse["numCivique"].Should().Be("");
        adresse["numUnite"].Should().Be("");
    }

    [Fact]
    public void VilleManquante_ClearsMunicipalite()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.VilleManquante, isIndividu: true);
        GetAdresse(root)["nomMunicipalite"].Should().Be("");
    }

    [Fact]
    public void CodePaysManquant_ClearsPays_Individu()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.CodePaysManquant, isIndividu: true);
        GetAdresse(root)["codePaysIso"].Should().Be("");
    }

    [Fact]
    public void CodePaysManquant_ClearsPays_Organisation()
    {
        var root = BuildOrganisationRoot();
        _sut.Apply(root, AnomalyKind.CodePaysManquant, isIndividu: false);
        GetAdresse(root)["codPaysIso"].Should().Be("");
    }

    // ========== Avertissement ==========

    [Fact]
    public void CodeLangueManquant_ClearsLangue()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.CodeLangueManquant, isIndividu: true);
        ((Dictionary<string, object>)root["information"])["codLangue"].Should().Be("");
    }

    // ========== Edge Cases ==========

    [Fact]
    public void Apply_DoesNotThrow_WhenIdentificationTypeNotFound()
    {
        var root = BuildIndividuRoot();
        var act = () => _sut.Apply(root, AnomalyKind.FIDManquant, isIndividu: false);
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_MultipleAnomalies_StackCorrectly()
    {
        var root = BuildIndividuRoot();
        _sut.Apply(root, AnomalyKind.NomBeneficiaireManquant, isIndividu: true);
        _sut.Apply(root, AnomalyKind.CodeDeviseErrone, isIndividu: true);
        GetParty(root)["nomFamille"].Should().Be("");
        ((Dictionary<string, object>)root["information"])["codDevise"].Should().Be("NON");
    }

    // ========== GetAnomalyForSequence ==========

    [Fact]
    public void GetAnomalyForSequence_ReturnsNull_WhenDisabled()
    {
        var config = new T5Rl3Config
        {
            NombreLignes = 10,
            Anomalies = new AnomalyConfig { Enabled = false }
        };
        var sut = new AnomalyService(config);

        sut.GetAnomalyForSequence(10).Should().BeNull();
    }

    [Fact]
    public void GetAnomalyForSequence_ReturnsAnomaly_ForLastRecords()
    {
        var config = new T5Rl3Config
        {
            NombreLignes = 10,
            Anomalies = new AnomalyConfig
            {
                Enabled = true,
                Bloquant = new AnomalyLevelConfig
                {
                    Nombre = 2,
                    Types = [AnomalyKind.NomBeneficiaireManquant, AnomalyKind.CodeDeviseErrone]
                }
            }
        };
        var sut = new AnomalyService(config);

        sut.GetAnomalyForSequence(8).Should().BeNull();
        sut.GetAnomalyForSequence(9).Should().Be(AnomalyKind.NomBeneficiaireManquant);
        sut.GetAnomalyForSequence(10).Should().Be(AnomalyKind.CodeDeviseErrone);
    }

    [Fact]
    public void GetAnomalyForSequence_RoundRobins_WhenMoreNombreThanTypes()
    {
        var config = new T5Rl3Config
        {
            NombreLignes = 10,
            Anomalies = new AnomalyConfig
            {
                Enabled = true,
                Bloquant = new AnomalyLevelConfig
                {
                    Nombre = 3,
                    Types = [AnomalyKind.NomBeneficiaireManquant]
                }
            }
        };
        var sut = new AnomalyService(config);

        sut.GetAnomalyForSequence(8).Should().Be(AnomalyKind.NomBeneficiaireManquant);
        sut.GetAnomalyForSequence(9).Should().Be(AnomalyKind.NomBeneficiaireManquant);
        sut.GetAnomalyForSequence(10).Should().Be(AnomalyKind.NomBeneficiaireManquant);
    }
}
