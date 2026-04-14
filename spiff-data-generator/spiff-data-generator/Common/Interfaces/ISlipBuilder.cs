using spiff_data_generator.Common.Models;

namespace spiff_data_generator.Common.Interfaces;

public interface ISlipBuilder<in TContext> where TContext : ISlipContext
{
    bool CanBuild(TContext context);
    Dictionary<string, object> Build(TContext context);
}
