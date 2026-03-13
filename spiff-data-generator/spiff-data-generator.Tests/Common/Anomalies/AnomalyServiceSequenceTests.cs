using FluentAssertions;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.T5Rl3.Config;
using Xunit;

namespace spiff_data_generator.Tests.Common.Anomalies;

public class AnomalyServiceSequenceTests
{
    [Fact]
    public void GetAnomalyForSequence_Disabled_ReturnsNull()
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
    public void GetAnomalyForSequence_EarlySequence_ReturnsNull()
    {
        var config = new T5Rl3Config
        {
            NombreLignes = 100,
            Anomalies = new AnomalyConfig
            {
                Enabled = true,
                Bloquant = new AnomalyLevelConfig
                {
                    Nombre = 2,
                    Types = [AnomalyKind.NomBeneficiaireManquant]
                }
            }
        };
        var sut = new AnomalyService(config);

        sut.GetAnomalyForSequence(1).Should().BeNull();
    }

    [Fact]
    public void GetAnomalyForSequence_LastSequences_ReturnsAnomalies()
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

        sut.GetAnomalyForSequence(9).Should().Be(AnomalyKind.NomBeneficiaireManquant);
        sut.GetAnomalyForSequence(10).Should().Be(AnomalyKind.CodeDeviseErrone);
    }

    [Fact]
    public void GetAnomalyForSequence_MultipleLevels_DistributesCorrectly()
    {
        var config = new T5Rl3Config
        {
            NombreLignes = 10,
            Anomalies = new AnomalyConfig
            {
                Enabled = true,
                Bloquant = new AnomalyLevelConfig
                {
                    Nombre = 1,
                    Types = [AnomalyKind.NomBeneficiaireManquant]
                },
                Importante = new AnomalyLevelConfig
                {
                    Nombre = 1,
                    Types = [AnomalyKind.NASManquant]
                }
            }
        };
        var sut = new AnomalyService(config);

        sut.GetAnomalyForSequence(9).Should().Be(AnomalyKind.NomBeneficiaireManquant);
        sut.GetAnomalyForSequence(10).Should().Be(AnomalyKind.NASManquant);
    }
}
