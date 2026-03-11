namespace spiff_data_generator.Api;

public interface IFtpService
{
    Task UploadFile(Stream sourceStream, string ftpPath, string ftpFileName, int retryCount, int delaiInSeconds);
}
