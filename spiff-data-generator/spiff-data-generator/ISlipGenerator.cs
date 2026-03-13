namespace spiff_data_generator;

public interface ISlipGenerator
{
    Dictionary<string, object> Generate(int seq);
}
