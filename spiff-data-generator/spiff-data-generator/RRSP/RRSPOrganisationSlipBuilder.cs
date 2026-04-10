using spiff_data_generator.Common;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.RRSP;

public sealed class RRSPOrganisationSlipBuilder : ISlipBuilder<RRSPSlipContext>
{
    private readonly IRandomService _random;

    public RRSPOrganisationSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(RRSPSlipContext context) => !context.IsIndividu;

    public Dictionary<string, object> Build(RRSPSlipContext context)
    {
        // TODO: Implement RRSP organisation slip structure
        throw new NotImplementedException("RRSP organisation builder not yet implemented");
    }
}
