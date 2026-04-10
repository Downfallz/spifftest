using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T4RIFRL2;

public sealed class T4RIFRL2IndividuSlipBuilder : ISlipBuilder<T4RIFRL2SlipContext>
{
    private readonly IRandomService _random;

    public T4RIFRL2IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T4RIFRL2SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(T4RIFRL2SlipContext context)
    {
        // TODO: Implement T4RIFRL2 individu slip structure
        throw new NotImplementedException("T4RIFRL2 individu builder not yet implemented");
    }
}
