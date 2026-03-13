namespace spiff_data_generator.Common.Interfaces;

public interface ISlipGenerator
{
    Dictionary<string, object> Generate(int seq);
}
