using spiff_data_generator.Common;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T4RSPRL2;

public sealed class T4RSPRL2OrganisationSlipBuilder : ISlipBuilder<T4RSPRL2SlipContext>
{
    private readonly IRandomService _random;

    public T4RSPRL2OrganisationSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T4RSPRL2SlipContext context) => !context.IsIndividu;

    public Dictionary<string, object> Build(T4RSPRL2SlipContext context)
    {
        // TODO: Implement T4RSPRL2 organisation slip structure
        throw new NotImplementedException("T4RSPRL2 organisation builder not yet implemented");
    }
}
