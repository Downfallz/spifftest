using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.NR4;

public sealed class NR4IndividuSlipBuilder : ISlipBuilder<NR4SlipContext>
{
    private readonly IRandomService _random;

    public NR4IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(NR4SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(NR4SlipContext context)
    {
        // TODO: Implement NR4 individu slip structure
        throw new NotImplementedException("NR4 individu builder not yet implemented");
    }
}
