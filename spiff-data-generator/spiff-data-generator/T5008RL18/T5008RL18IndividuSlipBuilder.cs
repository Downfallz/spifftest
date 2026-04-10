using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;

namespace spiff_data_generator.T5008RL18;

public sealed class T5008RL18IndividuSlipBuilder : ISlipBuilder<T5008RL18SlipContext>
{
    private readonly IRandomService _random;

    public T5008RL18IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(T5008RL18SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(T5008RL18SlipContext context)
    {
        // TODO: Implement T5008RL18 individu slip structure
        throw new NotImplementedException("T5008RL18 individu builder not yet implemented");
    }
}
