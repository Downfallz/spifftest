using FluentAssertions;
using Moq;
using Moq.AutoMock;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;
using Xunit;

namespace spiff_data_generator.Tests.T5Rl3.Builders;

public class SlipSchemaTests
{
    private readonly AutoMocker _mocker = new();

    private static SlipContext IndividuQcContext() => new(
        NumTransit: "81500008", NumCompte: "000001", Province: "QC",
        IsQc: true, Langue: "F", Pays: "CAN", TypImpression: "PN",
        HoldMail: false, Devise: "CAD", Case13: "1000.00",
        CaseD: "500.00", IsIndividu: true);

    private static SlipContext OrgQcContext() => new(
        NumTransit: "81500008", NumCompte: "000001", Province: "QC",
        IsQc: true, Langue: "F", Pays: "CAN", TypImpression: "PN",
        HoldMail: false, Devise: "CAD", Case13: "1000.00",
        CaseD: "500.00", IsIndividu: false);

    private void SetupMock()
    {
        var random = _mocker.GetMock<IRandomService>();
        random.Setup(r => r.FirstName()).Returns("JEAN");
        random.Setup(r => r.LastName()).Returns("TREMBLAY");
        random.Setup(r => r.CompanyName()).Returns("TEST INC");
        random.Setup(r => r.CompanySuffix()).Returns("LTEE");
        random.Setup(r => r.StreetName()).Returns("RUE TEST");
        random.Setup(r => r.City()).Returns("MONTREAL");
        random.Setup(r => r.BuildingNumber()).Returns("100");
        random.Setup(r => r.SecondaryAddress()).Returns("APT 1");
        random.Setup(r => r.GenerateSIN()).Returns("123456789");
        random.Setup(r => r.GenerateCanadianPostalCode(It.IsAny<string>())).Returns("H2X1Y4");
        random.Setup(r => r.FixedDigits(It.IsAny<int>())).Returns("12345678901");
        random.Setup(r => r.GenerateNEQ(It.IsAny<OrganisationType>())).Returns("3312345678");
        random.Setup(r => r.GenerateNI()).Returns("1234567890");
        random.Setup(r => r.RandomChoice(It.IsAny<IReadOnlyList<OrganisationType>>()))
            .Returns(OrganisationType.Societe);
    }

    // ── Root structure ─────────────────────────────────────

    [Fact]
    public void Individu_HasRootKeys()
    {
        SetupMock();
        var result = _mocker.CreateInstance<IndividuSlipBuilder>().Build(IndividuQcContext());

        result.Should().ContainKey("information");
        result.Should().ContainKey("contenu");
    }

    [Fact]
    public void Organisation_HasRootKeys()
    {
        SetupMock();
        var result = _mocker.CreateInstance<OrganisationSlipBuilder>().Build(OrgQcContext());

        result.Should().ContainKey("information");
        result.Should().ContainKey("contenu");
    }

    // ── Information schema ─────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Information_HasRequiredKeys(bool isIndividu)
    {
        SetupMock();
        var ctx = isIndividu ? IndividuQcContext() : OrgQcContext();
        var builder = isIndividu
            ? (ISlipBuilder)_mocker.CreateInstance<IndividuSlipBuilder>()
            : _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(ctx);
        var info = (Dictionary<string, object>)result["information"];

        info.Should().ContainKey("codFormulaireReleve");
        info.Should().ContainKey("codLangue");
        info.Should().ContainKey("codDevise");
        info.Should().ContainKey("typImpression");
        info.Should().ContainKey("holdMail");
        info.Should().ContainKey("numIdentificationEmetteur");
        info.Should().ContainKey("parties");
        info.Should().ContainKey("documents");
    }

    // ── Party schema ───────────────────────────────────────

    [Fact]
    public void IndividuParty_HasRequiredKeys()
    {
        SetupMock();
        var result = _mocker.CreateInstance<IndividuSlipBuilder>().Build(IndividuQcContext());
        var party = GetParty(result);

        party.Should().ContainKey("idCodSousTypePartie");
        party.Should().ContainKey("idCodRoleRelevePartie");
        party.Should().ContainKey("idCodTypeRoleRelevePartie");
        party.Should().ContainKey("identificationPartie");
        party.Should().ContainKey("prn");
        party.Should().ContainKey("nomFamille");
        party.Should().ContainKey("nomInitiale");
        party.Should().ContainKey("adresseFiscale");
        party.Should().ContainKey("indAdFiscalePostaleIdentique");
        party["idCodSousTypePartie"].Should().Be(1);
    }

    [Fact]
    public void OrgParty_HasRequiredKeys()
    {
        SetupMock();
        var result = _mocker.CreateInstance<OrganisationSlipBuilder>().Build(OrgQcContext());
        var party = GetParty(result);

        party.Should().ContainKey("idCodSousTypePartie");
        party.Should().ContainKey("idCodRoleRelevePartie");
        party.Should().ContainKey("idCodTypeRoleRelevePartie");
        party.Should().ContainKey("identificationPartie");
        party.Should().ContainKey("nomOrganisationLign1");
        party.Should().ContainKey("nomOrganisationLign2");
        party.Should().ContainKey("adresseFiscale");
        party.Should().ContainKey("indAdFiscalePostaleIdentique");
        party["idCodSousTypePartie"].Should().Be(2);
    }

    // ── Address schema ─────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Adresse_HasRequiredKeys(bool isIndividu)
    {
        SetupMock();
        var ctx = isIndividu ? IndividuQcContext() : OrgQcContext();
        var builder = isIndividu
            ? (ISlipBuilder)_mocker.CreateInstance<IndividuSlipBuilder>()
            : _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(ctx);
        var party = GetParty(result);
        var adresse = (Dictionary<string, object>)party["adresseFiscale"];

        adresse.Should().ContainKey("numCivique");
        adresse.Should().ContainKey("nomRue");
        adresse.Should().ContainKey("nomMunicipalite");
        adresse.Should().ContainKey("numUnite");
        adresse.Should().ContainKey("codProvince");
        adresse.Should().ContainKey("codPaysIso");
        adresse.Should().ContainKey("numCodPostal");
        // Must NOT contain old keys
        adresse.Should().NotContainKey("numCodePostal");
        adresse.Should().NotContainKey("codePaysIso");
    }

    // ── Identification schema ──────────────────────────────

    [Fact]
    public void IndividuIdentification_HasAccountAndSIN()
    {
        SetupMock();
        var result = _mocker.CreateInstance<IndividuSlipBuilder>().Build(IndividuQcContext());
        var idents = GetIdentifications(result);

        idents.Should().HaveCountGreaterOrEqualTo(2);
        var types = idents.Select(i => (int)i["idCodTypeIdentificationPartie"]).ToList();
        types.Should().Contain(1); // SIN
        types.Should().Contain(4); // Account
    }

    [Fact]
    public void Identification_Entries_HaveRequiredKeys()
    {
        SetupMock();
        var result = _mocker.CreateInstance<IndividuSlipBuilder>().Build(IndividuQcContext());
        var idents = GetIdentifications(result);

        foreach (var ident in idents)
        {
            ident.Should().ContainKey("idCodTypeIdentificationPartie");
            ident.Should().ContainKey("numIdentificationPartie");
        }
    }

    // ── Documents schema ───────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Documents_HasMetaDonneesDocument(bool isIndividu)
    {
        SetupMock();
        var ctx = isIndividu ? IndividuQcContext() : OrgQcContext();
        var builder = isIndividu
            ? (ISlipBuilder)_mocker.CreateInstance<IndividuSlipBuilder>()
            : _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(ctx);
        var info = (Dictionary<string, object>)result["information"];
        var docs = (List<object>)info["documents"];

        docs.Should().HaveCountGreaterOrEqualTo(1);
        var doc = (Dictionary<string, object>)docs[0];
        doc.Should().ContainKey("metaDonneesDocument");
        // Must NOT contain old key
        doc.Should().NotContainKey("metadonneesDocument");

        var meta = (List<object>)doc["metaDonneesDocument"];
        foreach (var entry in meta.Cast<Dictionary<string, object>>())
        {
            entry.Should().ContainKey("codTypeMetadonneeDocument");
            entry.Should().ContainKey("valMetadonneeDocument");
        }
    }

    // ── Contenu schema ─────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Contenu_HasCases(bool isIndividu)
    {
        SetupMock();
        var ctx = isIndividu ? IndividuQcContext() : OrgQcContext();
        var builder = isIndividu
            ? (ISlipBuilder)_mocker.CreateInstance<IndividuSlipBuilder>()
            : _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(ctx);
        var contenu = (Dictionary<string, object>)result["contenu"];
        var cases = (List<object>)contenu["cases"];

        cases.Should().HaveCountGreaterOrEqualTo(3);
        foreach (var entry in cases.Cast<Dictionary<string, object>>())
        {
            entry.Should().ContainKey("case");
            entry.Should().ContainKey("valeur");
        }
    }

    // ── Helpers ─────────────────────────────────────────────

    private static Dictionary<string, object> GetParty(Dictionary<string, object> root)
    {
        var info = (Dictionary<string, object>)root["information"];
        var parties = (List<object>)info["parties"];
        return (Dictionary<string, object>)parties[0];
    }

    private static List<Dictionary<string, object>> GetIdentifications(Dictionary<string, object> root)
    {
        var party = GetParty(root);
        var idents = (List<object>)party["identificationPartie"];
        return idents.Cast<Dictionary<string, object>>().ToList();
    }
}
