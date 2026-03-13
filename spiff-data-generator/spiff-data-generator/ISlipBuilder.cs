namespace spiff_data_generator;

public interface ISlipBuilder
{
    bool CanBuild(SlipContext context);
    Dictionary<string, object> Build(SlipContext context);
}
