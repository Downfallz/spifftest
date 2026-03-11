namespace spiff_data_generator.Api;

public sealed class LocalDiskFtpService : IFtpService
{
    private readonly ILogger<LocalDiskFtpService> _logger;

    public LocalDiskFtpService(ILogger<LocalDiskFtpService> logger)
    {
        _logger = logger;
    }

    public async Task UploadFile(Stream sourceStream, string ftpPath, string ftpFileName, int retryCount, int delaiInSeconds)
    {
        var fullDir = Path.GetFullPath(ftpPath);
        Directory.CreateDirectory(fullDir);
        var fullPath = Path.Combine(fullDir, ftpFileName);

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "[FTP MOCK] Tentative {Attempt}/{Max} - Upload de {FileName} vers {Path}",
                    attempt, retryCount, ftpFileName, ftpPath);

                sourceStream.Position = 0;
                await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                await sourceStream.CopyToAsync(fs);

                _logger.LogInformation(
                    "[FTP MOCK] Upload réussi: {FullPath} ({Size} bytes)",
                    fullPath, new FileInfo(fullPath).Length);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "[FTP MOCK] Échec tentative {Attempt}/{Max}. Retry dans {Delai}s...",
                    attempt, retryCount, delaiInSeconds);

                if (attempt < retryCount)
                    await Task.Delay(TimeSpan.FromSeconds(delaiInSeconds));
            }
        }

        throw new IOException($"Impossible d'uploader {ftpFileName} après {retryCount} tentatives.");
    }
}
