using FluentAssertions;
using Moq;
using Moq.AutoMock;
using spiff_data_generator.Common.Random;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;
using Xunit;

namespace spiff_data_generator.Tests.T5Rl3.Builders;

public class SlipBuilderTests
{
    private readonly AutoMocker _mocker = new();

    private static SlipContext QcIndividuContext() => new(
        NumTransit: "81500008",
        NumCompte: "000001",
        Province: "QC",
        IsQc: true,
        Langue: "F",
        Pays: "CAN",
        TypImpression: "PN",
        HoldMail: false,
        Devise: "CAD",
        Case13: "1000.00",
        CaseD: "500.00",
        IsIndividu: true);

    private static SlipContext OnOrganisationContext() => new(
        NumTransit: "32900303",
        NumCompte: "000002",
        Province: "ON",
        IsQc: false,
        Langue: "A",
        Pays: "CAN",
        TypImpression: "N",
        HoldMail: true,
        Devise: "USD",
        Case13: "2000.00",
        CaseD: "750.00",
        IsIndividu: false);

    private void SetupRandomMock()
    {
        var random = _mocker.GetMock<IRandomService>();
        random.Setup(r => r.FirstName()).Returns("JEAN");
        random.Setup(r => r.LastName()).Returns("TREMBLAY");
        random.Setup(r => r.CompanyName()).Returns("DESJARDINS INC");
        random.Setup(r => r.CompanySuffix()).Returns("LTEE");
        random.Setup(r => r.StreetName()).Returns("RUE PRINCIPALE");
        random.Setup(r => r.City()).Returns("QUEBEC");
        random.Setup(r => r.BuildingNumber()).Returns("123");
        random.Setup(r => r.SecondaryAddress()).Returns("APT 2");
        random.Setup(r => r.GenerateSIN()).Returns("123456789");
        random.Setup(r => r.GenerateNEQ(It.IsAny<OrganisationType>())).Returns("3312345678");
        random.Setup(r => r.GenerateNI()).Returns("1234567890");
        random.Setup(r => r.GenerateAccount()).Returns("9876543210");
        random.Setup(r => r.GenerateCanadianPostalCode(It.IsAny<string>())).Returns("G1K2A3");
        random.Setup(r => r.FixedDigits(It.IsAny<int>())).Returns("12345678901");
        random.Setup(r => r.RandomChoice(It.IsAny<IReadOnlyList<OrganisationType>>()))
            .Returns(OrganisationType.Societe);
    }

    // ========== IndividuSlipBuilder ==========

    [Fact]
    public void IndividuSlipBuilder_CanBuild_ReturnsTrueForIndividu()
    {
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();
        builder.CanBuild(QcIndividuContext()).Should().BeTrue();
    }

    [Fact]
    public void IndividuSlipBuilder_CanBuild_ReturnsFalseForOrganisation()
    {
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();
        builder.CanBuild(OnOrganisationContext()).Should().BeFalse();
    }

    [Fact]
    public void IndividuSlipBuilder_Build_HasRequiredKeys()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();

        var result = builder.Build(QcIndividuContext());

        result.Should().ContainKey("information");
        result.Should().ContainKey("contenu");
        var info = (Dictionary<string, object>)result["information"];
        info.Should().ContainKey("documents");
    }

    [Fact]
    public void IndividuSlipBuilder_Build_SetsFormulaire_QC()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();

        var result = builder.Build(QcIndividuContext());
        var info = (Dictionary<string, object>)result["information"];

        info["codFormulaireReleve"].Should().Be("T5RL3");
        info["codLangue"].Should().Be("F");
    }

    [Fact]
    public void IndividuSlipBuilder_Build_SetsSINInParties()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();

        var result = builder.Build(QcIndividuContext());
        var info = (Dictionary<string, object>)result["information"];
        var parties = (List<object>)info["parties"];
        var party = (Dictionary<string, object>)parties[0];
        var idents = (List<object>)party["identificationPartie"];
        var sin = (Dictionary<string, object>)idents[0];

        sin["idCodTypeIdentificationPartie"].Should().Be(1);
        sin["numIdentificationPartie"].Should().Be("123456789");
    }

    [Fact]
    public void IndividuSlipBuilder_Build_IncludesCases_QC()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<IndividuSlipBuilder>();

        var result = builder.Build(QcIndividuContext());
        var contenu = (Dictionary<string, object>)result["contenu"];
        var cases = (List<object>)contenu["cases"];

        cases.Should().HaveCountGreaterOrEqualTo(4); // 13, 28, 29, D, Succ
    }

    // ========== OrganisationSlipBuilder ==========

    [Fact]
    public void OrganisationSlipBuilder_CanBuild_ReturnsTrueForOrganisation()
    {
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();
        builder.CanBuild(OnOrganisationContext()).Should().BeTrue();
    }

    [Fact]
    public void OrganisationSlipBuilder_CanBuild_ReturnsFalseForIndividu()
    {
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();
        builder.CanBuild(QcIndividuContext()).Should().BeFalse();
    }

    [Fact]
    public void OrganisationSlipBuilder_Build_HasRequiredKeys()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(OnOrganisationContext());

        result.Should().ContainKey("information");
        result.Should().ContainKey("contenu");
    }

    [Fact]
    public void OrganisationSlipBuilder_Build_SetsFormulaireReleve()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(OnOrganisationContext());
        var info = (Dictionary<string, object>)result["information"];

        info["codFormulaireReleve"].Should().Be("T5");
        info["codLangue"].Should().Be("A");
    }

    [Fact]
    public void OrganisationSlipBuilder_Build_SetsCompanyName()
    {
        SetupRandomMock();
        var builder = _mocker.CreateInstance<OrganisationSlipBuilder>();

        var result = builder.Build(OnOrganisationContext());
        var info = (Dictionary<string, object>)result["information"];
        var parties = (List<object>)info["parties"];
        var party = (Dictionary<string, object>)parties[0];

        party["nomOrganisationLign1"].Should().Be("DESJARDINS INC");
        party["nomOrganisationLign2"].Should().Be("LTEE");
    }
}
