using FluentAssertions;
using Moq;
using Moq.AutoMock;
using spiff_data_generator.Common.Random;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;
using Xunit;

namespace spiff_data_generator.Tests.T5Rl3.Builders;

public class OrganisationIdentificationTests
{
    private readonly AutoMocker _mocker = new();

    private static SlipContext QcOrgContext() => new(
        NumTransit: "81500008", NumCompte: "000001", Province: "QC",
        IsQc: true, Langue: "F", Pays: "CAN", TypImpression: "PN",
        HoldMail: false, Devise: "CAD", Case13: "1000.00",
        CaseD: "500.00", IsIndividu: false);

    private static SlipContext NonQcOrgContext() => new(
        NumTransit: "32900303", NumCompte: "000002", Province: "ON",
        IsQc: false, Langue: "A", Pays: "CAN", TypImpression: "N",
        HoldMail: false, Devise: "USD", Case13: "2000.00",
        CaseD: "750.00", IsIndividu: false);

    private void SetupMock(OrganisationType genre)
    {
        var random = _mocker.GetMock<IRandomService>();
        random.Setup(r => r.CompanyName()).Returns("TEST INC");
        random.Setup(r => r.CompanySuffix()).Returns("LTEE");
        random.Setup(r => r.StreetName()).Returns("RUE TEST");
        random.Setup(r => r.City()).Returns("MONTREAL");
        random.Setup(r => r.BuildingNumber()).Returns("100");
        random.Setup(r => r.SecondaryAddress()).Returns("APT 1");
        random.Setup(r => r.GenerateCanadianPostalCode(It.IsAny<string>())).Returns("H2X1Y4");
        random.Setup(r => r.FixedDigits(It.IsAny<int>())).Returns("12345678901");
        random.Setup(r => r.GenerateNEQ(It.IsAny<OrganisationType>())).Returns("3312345678");
        random.Setup(r => r.GenerateNI()).Returns("1234567890");
        random.Setup(r => r.RandomChoice(It.IsAny<IReadOnlyList<OrganisationType>>())).Returns(genre);
    }

    private List<object> GetIdentification(SlipContext context)
    {
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();
        var result = builder.Build(context);
        var info = (Dictionary<string, object>)result["information"];
        var parties = (List<object>)info["parties"];
        var party = (Dictionary<string, object>)parties[0];
        return (List<object>)party["identificationPartie"];
    }

    private static int GetIdType(List<object> idents, int index) =>
        (int)((Dictionary<string, object>)idents[index])["idCodTypeIdentificationPartie"];

    [Fact]
    public void Societe_QC_HasNE_And_NEQ()
    {
        SetupMock(OrganisationType.Societe);
        var idents = GetIdentification(QcOrgContext());

        // Type 4 (compte), Type 2 (NE), Type 6 (NEQ)
        idents.Should().HaveCount(3);
        GetIdType(idents, 0).Should().Be(4);
        GetIdType(idents, 1).Should().Be(2);
        GetIdType(idents, 2).Should().Be(6);
    }

    [Fact]
    public void Societe_NonQC_HasNE_Only()
    {
        SetupMock(OrganisationType.Societe);
        var idents = GetIdentification(NonQcOrgContext());

        // Type 4 (compte), Type 2 (NE)
        idents.Should().HaveCount(2);
        GetIdType(idents, 0).Should().Be(4);
        GetIdType(idents, 1).Should().Be(2);
    }

    [Fact]
    public void Fiducie_QC_HasNEQ_And_NI()
    {
        SetupMock(OrganisationType.Fiducie);
        var idents = GetIdentification(QcOrgContext());

        // Type 4 (compte), Type 6 (NEQ), Type 7 (NI)
        idents.Should().HaveCount(3);
        GetIdType(idents, 0).Should().Be(4);
        GetIdType(idents, 1).Should().Be(6);
        GetIdType(idents, 2).Should().Be(7);
    }

    [Fact]
    public void Fiducie_NonQC_HasFID()
    {
        SetupMock(OrganisationType.Fiducie);
        var idents = GetIdentification(NonQcOrgContext());

        // Type 4 (compte), Type 8 (FID)
        idents.Should().HaveCount(2);
        GetIdType(idents, 0).Should().Be(4);
        GetIdType(idents, 1).Should().Be(8);
    }

    [Fact]
    public void Association_QC_HasNE_And_NEQ()
    {
        SetupMock(OrganisationType.Association);
        var idents = GetIdentification(QcOrgContext());

        // Type 4 (compte), Type 2 (NE), Type 6 (NEQ)
        idents.Should().HaveCount(3);
        GetIdType(idents, 0).Should().Be(4);
        GetIdType(idents, 1).Should().Be(2);
        GetIdType(idents, 2).Should().Be(6);
    }

    [Fact]
    public void Fiducie_HasPDO_InDocuments()
    {
        SetupMock(OrganisationType.Fiducie);
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();
        var result = builder.Build(QcOrgContext());
        var info = (Dictionary<string, object>)result["information"];
        var docs = (List<object>)info["documents"];
        var doc = (Dictionary<string, object>)docs[0];
        var meta = (List<object>)doc["metaDonneesDocument"];

        meta.Cast<Dictionary<string, object>>()
            .Should().Contain(m => (string)m["codTypeMetadonneeDocument"] == "numIdentifiantPDO");
    }

    [Fact]
    public void Societe_DoesNotHave_PDO_InDocuments()
    {
        SetupMock(OrganisationType.Societe);
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();
        var result = builder.Build(QcOrgContext());
        var info = (Dictionary<string, object>)result["information"];
        var docs = (List<object>)info["documents"];
        var doc = (Dictionary<string, object>)docs[0];
        var meta = (List<object>)doc["metaDonneesDocument"];

        meta.Cast<Dictionary<string, object>>()
            .Should().NotContain(m => (string)m["codTypeMetadonneeDocument"] == "numIdentifiantPDO");
    }
}
