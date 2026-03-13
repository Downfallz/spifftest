using FluentAssertions;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Models;
using Xunit;

namespace spiff_data_generator.Tests.T5Rl3.Builders;

public class CaseBuilderTests
{
    private static SlipContext QcContext() => new(
        NumTransit: "81500008", NumCompte: "000001", Province: "QC",
        IsQc: true, Langue: "F", Pays: "CAN", TypImpression: "PN",
        HoldMail: false, Devise: "CAD", Case13: "1000.00",
        CaseD: "500.00", IsIndividu: true);

    private static SlipContext NonQcContext() => new(
        NumTransit: "32900303", NumCompte: "000002", Province: "ON",
        IsQc: false, Langue: "A", Pays: "CAN", TypImpression: "N",
        HoldMail: false, Devise: "USD", Case13: "2000.00",
        CaseD: "750.00", IsIndividu: true);

    [Fact]
    public void Build_QC_Returns5Cases()
    {
        var cases = CaseBuilder.Build(QcContext());
        cases.Should().HaveCount(5);
    }

    [Fact]
    public void Build_NonQC_Returns3Cases()
    {
        var cases = CaseBuilder.Build(NonQcContext());
        cases.Should().HaveCount(3);
    }

    [Fact]
    public void Build_Case13_HasCorrectValue()
    {
        var cases = CaseBuilder.Build(QcContext());
        var case13 = (Dictionary<string, object>)cases[0];
        case13["case"].Should().Be("13");
        case13["valeur"].Should().Be("1000.00");
    }

    [Fact]
    public void Build_Case28_HasTransitNumber()
    {
        var cases = CaseBuilder.Build(QcContext());
        var case28 = (Dictionary<string, object>)cases[1];
        case28["case"].Should().Be("28");
        case28["valeur"].Should().Be("81500008");
    }

    [Fact]
    public void Build_Case29_HasAccountNumber()
    {
        var cases = CaseBuilder.Build(QcContext());
        var case29 = (Dictionary<string, object>)cases[2];
        case29["case"].Should().Be("29");
        case29["valeur"].Should().Be("000001");
    }

    [Fact]
    public void Build_QC_HasCaseD()
    {
        var cases = CaseBuilder.Build(QcContext());
        var caseD = (Dictionary<string, object>)cases[3];
        caseD["case"].Should().Be("D");
        caseD["valeur"].Should().Be("500.00");
    }

    [Fact]
    public void Build_QC_HasCaseSucc()
    {
        var cases = CaseBuilder.Build(QcContext());
        var caseSucc = (Dictionary<string, object>)cases[4];
        caseSucc["case"].Should().Be("Succ");
        caseSucc["valeur"].Should().Be("81500008");
    }
}
