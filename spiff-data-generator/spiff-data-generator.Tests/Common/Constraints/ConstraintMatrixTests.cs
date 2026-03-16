using FluentAssertions;
using spiff_data_generator.Common.Constraints;
using spiff_data_generator.T5Rl3.Models;
using Xunit;

namespace spiff_data_generator.Tests.Common.Constraints;

public class ConstraintMatrixTests
{
    private static ConstraintConfig DefaultConfig() => new()
    {
        Enabled = true,
        PoidsReglementaire = 25,
        PoidsTypeBeneficiaire = 15,
        PoidsComplexiteIdentification = 20,
        PoidsDeviseEtrangere = 15,
        PoidsContrainteImpression = 15,
        PoidsCourrierRetenu = 10,
        SeuilModere = 26,
        SeuilEleve = 51,
        SeuilCritique = 76,
    };

    private static SlipContext MakeContext(
        bool isQc = false,
        bool isIndividu = true,
        string devise = "CAD",
        string typImpression = "N",
        bool holdMail = false) => new(
            NumTransit: "00112345",
            NumCompte: "000001",
            Province: isQc ? "QC" : "ON",
            IsQc: isQc,
            Langue: isQc ? "F" : "A",
            Pays: "CAN",
            TypImpression: typImpression,
            HoldMail: holdMail,
            Devise: devise,
            Case13: "100.00",
            CaseD: "50.00",
            IsIndividu: isIndividu);

    [Fact]
    public void Evaluate_SimplestProfile_ScoreIsZero()
    {
        // Individu, hors-QC, CAD, électronique, pas de courrier retenu
        var context = MakeContext();
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Score.Should().Be(0);
        result.Niveau.Should().Be(ConstraintNiveau.Faible);
    }

    [Fact]
    public void Evaluate_MaxConstraintProfile_ScoreIs100()
    {
        // Org, QC, devise étrangère, impression PN, courrier retenu
        var context = MakeContext(
            isQc: true, isIndividu: false, devise: "USD",
            typImpression: "PN", holdMail: true);

        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Score.Should().Be(100);
        result.Niveau.Should().Be(ConstraintNiveau.Critique);
    }

    [Fact]
    public void Evaluate_QcIndividu_OnlyReglementaire()
    {
        var context = MakeContext(isQc: true);
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Details[ConstraintDimension.ReglementaireProvincial].Should().Be(25);
        result.Details[ConstraintDimension.TypeBeneficiaire].Should().Be(0);
        result.Score.Should().Be(25);
        result.Niveau.Should().Be(ConstraintNiveau.Faible);
    }

    [Fact]
    public void Evaluate_OrgHorsQc_TypeBeneficiaireEtDemiIdentification()
    {
        var context = MakeContext(isIndividu: false);
        var config = DefaultConfig();
        var result = ConstraintMatrix.Evaluate(context, config);

        result.Details[ConstraintDimension.TypeBeneficiaire].Should().Be(15);
        // Org hors-QC = moitié du poids d'identification
        result.Details[ConstraintDimension.ComplexiteIdentification].Should().Be(10);
        result.Details[ConstraintDimension.ReglementaireProvincial].Should().Be(0);
    }

    [Fact]
    public void Evaluate_OrgQc_FullIdentificationWeight()
    {
        var context = MakeContext(isQc: true, isIndividu: false);
        var config = DefaultConfig();
        var result = ConstraintMatrix.Evaluate(context, config);

        result.Details[ConstraintDimension.ComplexiteIdentification].Should().Be(20);
    }

    [Fact]
    public void Evaluate_DeviseEtrangere_AddsWeight()
    {
        var context = MakeContext(devise: "EUR");
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Details[ConstraintDimension.DeviseEtrangere].Should().Be(15);
    }

    [Fact]
    public void Evaluate_ImpressionPN_AddsWeight()
    {
        var context = MakeContext(typImpression: "PN");
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Details[ConstraintDimension.ContrainteImpression].Should().Be(15);
    }

    [Fact]
    public void Evaluate_CourrierRetenu_AddsWeight()
    {
        var context = MakeContext(holdMail: true);
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Details[ConstraintDimension.CourrierRetenu].Should().Be(10);
    }

    [Fact]
    public void Evaluate_NiveauModere()
    {
        // QC + devise étrangère = 25 + 15 = 40 raw, 40/100 = 40 normalized
        var context = MakeContext(isQc: true, devise: "USD");
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Score.Should().Be(40);
        result.Niveau.Should().Be(ConstraintNiveau.Modere);
    }

    [Fact]
    public void Evaluate_NiveauEleve()
    {
        // QC + org + devise étrangère = 25 + 15 + 20 + 15 = 75 raw → 75 normalized
        var context = MakeContext(isQc: true, isIndividu: false, devise: "USD");
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Score.Should().Be(75);
        result.Niveau.Should().Be(ConstraintNiveau.Eleve);
    }

    [Fact]
    public void Evaluate_CustomWeights_AffectsScore()
    {
        var config = DefaultConfig();
        config.PoidsReglementaire = 50;
        config.PoidsTypeBeneficiaire = 50;
        config.PoidsComplexiteIdentification = 0;
        config.PoidsDeviseEtrangere = 0;
        config.PoidsContrainteImpression = 0;
        config.PoidsCourrierRetenu = 0;

        var context = MakeContext(isQc: true);
        var result = ConstraintMatrix.Evaluate(context, config);

        // 50 / 100 total = 50 normalized
        result.Score.Should().Be(50);
    }

    [Fact]
    public void Evaluate_AllWeightsZero_ScoreIsZero()
    {
        var config = DefaultConfig();
        config.PoidsReglementaire = 0;
        config.PoidsTypeBeneficiaire = 0;
        config.PoidsComplexiteIdentification = 0;
        config.PoidsDeviseEtrangere = 0;
        config.PoidsContrainteImpression = 0;
        config.PoidsCourrierRetenu = 0;

        var context = MakeContext(isQc: true, isIndividu: false, devise: "EUR",
            typImpression: "PN", holdMail: true);
        var result = ConstraintMatrix.Evaluate(context, config);

        result.Score.Should().Be(0);
    }

    [Fact]
    public void Evaluate_DetailsContainsAllDimensions()
    {
        var context = MakeContext();
        var result = ConstraintMatrix.Evaluate(context, DefaultConfig());

        result.Details.Should().ContainKeys(
            ConstraintDimension.ReglementaireProvincial,
            ConstraintDimension.TypeBeneficiaire,
            ConstraintDimension.ComplexiteIdentification,
            ConstraintDimension.DeviseEtrangere,
            ConstraintDimension.ContrainteImpression,
            ConstraintDimension.CourrierRetenu);
    }
}
