using spiff_data_generator.T5Rl3.Config;

namespace spiff_data_generator.Common.RandomGen;

public interface IRandomService
{
    T RandomChoice<T>(IReadOnlyList<T> vals);
    T WeightedChoice<T>(IReadOnlyList<T> vals, IReadOnlyList<int> weights);
    string FixedDigits(int length);
    string RandomDecimal(int minLeft, int maxLeft, int decimals);

    // Identifiants
    string GenerateSIN();
    string GenerateNEQ(OrganisationType genre);
    string GenerateNI();
    string GenerateAccount();
    string GenerateCanadianPostalCode(string province);

    // Bogus wrappers
    string FirstName();
    string LastName();
    string CompanyName();
    string CompanySuffix();
    string StreetName();
    string City();
    string BuildingNumber();
    string SecondaryAddress();
}
