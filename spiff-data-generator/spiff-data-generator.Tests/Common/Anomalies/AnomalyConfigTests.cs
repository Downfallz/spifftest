using FluentAssertions;
using spiff_data_generator.Common.Anomalies;
using Xunit;

namespace spiff_data_generator.Tests.Common.Anomalies;

public class AnomalyConfigTests
{
    [Fact]
    public void DefaultConfig_HasAnomaliesDisabled()
    {
        var cfg = new T5Rl3Config();
        cfg.Anomalies.Should().NotBeNull();
        cfg.Anomalies.Enabled.Should().BeFalse();
    }

    [Fact]
    public void DefaultLevelConfig_HasZeroNombreAndEmptyTypes()
    {
        var level = new AnomalyLevelConfig();
        level.Nombre.Should().Be(0);
        level.Types.Should().BeEmpty();
    }

    [Fact]
    public void AnomalyConfig_AllLevels_AreInitialized()
    {
        var cfg = new AnomalyConfig();
        cfg.Bloquant.Should().NotBeNull();
        cfg.Importante.Should().NotBeNull();
        cfg.SevereImpression.Should().NotBeNull();
        cfg.Avertissement.Should().NotBeNull();
    }

    [Fact]
    public void LevelConfig_Types_AcceptEnumValues()
    {
        var level = new AnomalyLevelConfig
        {
            Nombre = 2,
            Types = [AnomalyKind.NomBeneficiaireManquant, AnomalyKind.CodeDeviseErrone]
        };

        level.Types.Should().HaveCount(2);
        level.Types[0].Should().Be(AnomalyKind.NomBeneficiaireManquant);
    }
}
