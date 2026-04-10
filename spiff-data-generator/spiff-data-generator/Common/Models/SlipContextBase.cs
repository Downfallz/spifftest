using spiff_data_generator.Common.Models;

namespace spiff_data_generator.T5Rl3.Models;

public record SlipContextBase(
    string Province, string Pays) : ISlipContext;
