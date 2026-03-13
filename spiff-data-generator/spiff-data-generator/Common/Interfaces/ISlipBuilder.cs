using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.Common.Interfaces;

public interface ISlipBuilder
{
    bool CanBuild(SlipContext context);
    Dictionary<string, object> Build(SlipContext context);
}
