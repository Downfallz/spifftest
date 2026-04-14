namespace spiff_data_generator.Common.Models;

public record SlipContextBase(
    string Province, string Pays) : ISlipContext;
