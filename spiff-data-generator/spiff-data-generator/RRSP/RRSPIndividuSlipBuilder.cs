using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.RRSP;

public sealed class RRSPIndividuSlipBuilder : ISlipBuilder<RRSPSlipContext>
{
    private readonly IRandomService _random;

    public RRSPIndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(RRSPSlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(RRSPSlipContext context)
    {
        // TODO: Implement RRSP individu slip structure
        throw new NotImplementedException("RRSP individu builder not yet implemented");
    }
}
