using Bogus;
using Microsoft.Extensions.Configuration;
using spiff_data_generator;

public static class Program
{
    public static void Main(string[] args)
    {
        // 1) Charger la config depuis appsettings.json (et env)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var cfg = configuration.GetSection(key: "T5Rl3").Get<T5Rl3Config>() ?? new T5Rl3Config();

        // 2) Seed Bogus + RNG
        Randomizer.Seed = new Random(cfg.Seed);
        var generator = new T5Rl3Generator(cfg);

        // 3) Générer vers fichier (ZIP en streaming)
        generator.GenerateToFile();

        Console.WriteLine("Terminé.");
    }
}
