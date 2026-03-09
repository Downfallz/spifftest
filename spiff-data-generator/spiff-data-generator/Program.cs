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

        var cfg = configuration.GetSection(key: "T5RI3").Get<T5RI3Config>() ?? new T5RI3Config();

        // 2) Seed Bogus + RNG
        Randomizer.Seed = new Random(cfg.Seed);
        var generator = new T5RI3Generator(cfg);

        // 3) Générer vers fichier (ZIP en streaming)
        generator.GenerateToFile();

        Console.WriteLine("Terminé.");
    }
}
