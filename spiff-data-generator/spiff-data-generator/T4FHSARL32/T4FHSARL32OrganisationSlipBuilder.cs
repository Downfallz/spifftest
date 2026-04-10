using spiff_data_generator.Common;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T4FHSARL32;

public sealed class T4FHSARL32OrganisationSlipBuilder : ISlipBuilder<T4FHSARL32SlipContext>
{
    private readonly IRandomService _random;

    public T4FHSARL32OrganisationSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T4FHSARL32SlipContext context) => !context.IsIndividu;

    public Dictionary<string, object> Build(T4FHSARL32SlipContext context)
    {
        // TODO: Implement T4FHSARL32 organisation slip structure
        throw new NotImplementedException("T4FHSARL32 organisation builder not yet implemented");
    }
}
