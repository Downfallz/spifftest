using spiff_data_generator.Common;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.NR4;

public sealed class NR4OrganisationSlipBuilder : ISlipBuilder<NR4SlipContext>
{
    private readonly IRandomService _random;

    public NR4OrganisationSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(NR4SlipContext context) => !context.IsIndividu;

    public Dictionary<string, object> Build(NR4SlipContext context)
    {
        // TODO: Implement NR4 organisation slip structure
        throw new NotImplementedException("NR4 organisation builder not yet implemented");
    }
}
