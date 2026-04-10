using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T4RSPRL2;

public sealed class T4RSPRL2IndividuSlipBuilder : ISlipBuilder<T4RSPRL2SlipContext>
{
    private readonly IRandomService _random;

    public T4RSPRL2IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T4RSPRL2SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(T4RSPRL2SlipContext context)
    {
        // TODO: Implement T4RSPRL2 individu slip structure
        throw new NotImplementedException("T4RSPRL2 individu builder not yet implemented");
    }
}
