namespace spiff_data_generator.Common.Export;

public interface IZipExporter
{
    void ExportToFile();
    void ExportToStream(Stream output, string? yyyymmdd = null);
}
