using FluentAssertions;
using spiff_data_generator;
using Xunit;

namespace spiff_data_generator.Tests;

public class AnomalyConfigTests
{
    [Fact]
    public void DefaultConfig_ShouldHaveAnomaliesDisabled()
    {
        var cfg = new T5Rl3Config();

        cfg.Anomalies.Should().NotBeNull();
        cfg.Anomalies.Enabled.Should().BeFalse();
    }

    [Fact]
    public void DefaultLevelConfig_ShouldHaveZeroNombreAndEmptyTypes()
    {
        var level = new AnomalyLevelConfig();

        level.Nombre.Should().Be(0);
        level.Types.Should().BeEmpty();
    }

    [Fact]
    public void AnomalyConfig_AllLevels_ShouldBeInitialized()
    {
        var anomCfg = new AnomalyConfig();

        anomCfg.Bloquant.Should().NotBeNull();
        anomCfg.Importante.Should().NotBeNull();
        anomCfg.SevereImpression.Should().NotBeNull();
        anomCfg.Avertissement.Should().NotBeNull();
    }
}
