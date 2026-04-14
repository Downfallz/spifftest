using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.Common.Models;

namespace spiff_data_generator.Common.Builders;
public static class AdresseHelper
{
    public static Dictionary<string, object> BuildAdresse(IRandomService random, SlipContextBase context)
    {
        return new Dictionary<string, object>
        {
            { "numCivique", random.BuildingNumber() },
            { "nomRue", random.StreetName() },
            { "nomMunicipalite", random.City() },
            { "numUnite", random.SecondaryAddress() },
            { "codProvince", context.Province },
            { "codPaysIso", context.Pays },
            { "numCodPostal", random.GenerateCanadianPostalCode(context.Province).Replace(" ", "") },
        };
    }
   
}
