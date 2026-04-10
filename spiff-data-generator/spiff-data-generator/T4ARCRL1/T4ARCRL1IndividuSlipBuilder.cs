using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T4ARCRL1;

public sealed class T4ARCRL1IndividuSlipBuilder : ISlipBuilder<T4ARCRL1SlipContext>
{
    private readonly IRandomService _random;

    public T4ARCRL1IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T4ARCRL1SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(T4ARCRL1SlipContext context)
    {
        // TODO: Implement T4ARCRL1 individu slip structure
        throw new NotImplementedException("T4ARCRL1 individu builder not yet implemented");
    }
}
