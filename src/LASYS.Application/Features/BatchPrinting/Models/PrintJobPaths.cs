namespace LASYS.Application.Features.BatchPrinting.Models
{
    public sealed record PrintJobPaths(
        string Root,
        string LabelsDirectory,
        string MetadataFilePath,
        string BatchLogPath);
}
