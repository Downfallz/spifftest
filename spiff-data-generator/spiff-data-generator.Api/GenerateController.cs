using Bogus;
using Microsoft.AspNetCore.Mvc;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Generation;

namespace spiff_data_generator.Api;

[ApiController]
[Route("api/[controller]")]
public class GenerateController : ControllerBase
{
    private readonly IFtpService _ftpService;
    private readonly ILogger<GenerateController> _logger;

    public GenerateController(IFtpService ftpService, ILogger<GenerateController> logger)
    {
        _ftpService = ftpService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(GenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest? request = null)
    {
        request ??= new GenerateRequest();

        // ── Validation ────────────────────────────────────
        var errors = new List<string>();

        if (request.NombreIndividus > request.NombreLignes)
            errors.Add("NombreIndividus ne peut pas dépasser NombreLignes.");

        if (request.WeightsCourrierRetenu.Length != 2)
            errors.Add("WeightsCourrierRetenu doit contenir exactement 2 éléments.");
        else if (request.WeightsCourrierRetenu.Sum() <= 0)
            errors.Add("La somme de WeightsCourrierRetenu doit être > 0.");

        if (request.WeightsImpression.Length != 2)
            errors.Add("WeightsImpression doit contenir exactement 2 éléments.");
        else if (request.WeightsImpression.Sum() <= 0)
            errors.Add("La somme de WeightsImpression doit être > 0.");

        if (request.WeightsCodeProvince.Length != 2)
            errors.Add("WeightsCodeProvince doit contenir exactement 2 éléments.");
        else if (request.WeightsCodeProvince.Sum() <= 0)
            errors.Add("La somme de WeightsCodeProvince doit être > 0.");

        if (request.Devises.Length == 0)
            errors.Add("Devises ne peut pas être vide.");

        if (request.BatchSize > request.NombreLignes)
            errors.Add("BatchSize ne peut pas dépasser NombreLignes.");

        if (errors.Count > 0)
            return BadRequest(new { errors });

        var config = request.ToConfig();
        Randomizer.Seed = new Random(config.Seed);

        // Build a scoped DI container for this request
        var random = new RandomService(config);
        var anomalyService = new AnomalyService(config);
        var builders = new ISlipBuilder[]
        {
            new IndividuSlipBuilder(random),
            new OrganisationSlipBuilder(random),
        };
        using var genLogger = new MsLoggerGenerationLogger(
            HttpContext.RequestServices.GetRequiredService<ILogger<MsLoggerGenerationLogger>>());
        var slipGenerator = new SlipGenerator(config, random, builders, anomalyService, genLogger);
        var zipExporter = new ZipExporter(config, slipGenerator, genLogger);

        _logger.LogInformation(
            "Génération démarrée: {Lignes} lignes ({Individus} individus, {Orgs} organisations)",
            config.NombreLignes, config.NombreIndividus, config.NombreLignes - config.NombreIndividus);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Generate ZIP in memory
        using var zipStream = new MemoryStream();
        var dateString = DateTime.Now.ToString("yyyyMMddHHmmss");
        zipExporter.ExportToStream(zipStream, dateString);

        sw.Stop();

        var fileName = $"{config.GetOutputPrefix()}_{dateString}.zip";

        _logger.LogInformation(
            "Génération terminée en {Elapsed}ms. Upload vers FTP...",
            sw.ElapsedMilliseconds);

        // Upload to FTP (mock = local disk)
        await _ftpService.UploadFile(
            zipStream,
            request.FtpPath,
            fileName,
            request.FtpRetryCount,
            request.FtpDelaiSeconds);

        return Ok(new GenerateResponse
        {
            FileName = fileName,
            FileSizeBytes = zipStream.Length,
            GenerationTimeMs = sw.ElapsedMilliseconds,
            NombreLignes = config.NombreLignes,
            FtpPath = request.FtpPath,
        });
    }
}

public sealed class GenerateResponse
{
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public long GenerationTimeMs { get; set; }
    public int NombreLignes { get; set; }
    public string FtpPath { get; set; } = "";
}
