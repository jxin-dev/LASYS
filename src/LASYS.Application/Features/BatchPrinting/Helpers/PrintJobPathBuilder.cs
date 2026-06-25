using LASYS.Application.Features.BatchPrinting.Models;
using LASYS.Application.Features.PrintLabels.Helpers;

namespace LASYS.Application.Features.BatchPrinting.Helpers
{
    public static class PrintJobPathBuilder
    {
        public static PrintJobPaths Create(string itemCode,string lotNo, string boxType)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            var printLabelsDirectory = NiceLabelFilePathBuilder.GetPrintJobsDirectory();

            var root = Path.Combine(printLabelsDirectory, Clean(itemCode), $"LOT-{Clean(lotNo)}_{boxType}", timestamp);
            var labelsDirectory = Path.Combine(root, "labels");

            Directory.CreateDirectory(labelsDirectory);

            return new PrintJobPaths(
                Root: root,
                LabelsDirectory: labelsDirectory,
                MetadataFilePath: Path.Combine(
                    root,
                    "metadata.json"),
                BatchLogPath: Path.Combine(
                    root,
                    "batch.log"));
        }
        private static string Clean(string value)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '-');
            }

            return value.Trim();
        }

        public static string CreateSampleDirectory(string itemCode)
        {
            var printLabelsDirectory = NiceLabelFilePathBuilder.GetPrintJobsDirectory();

            var root = Path.Combine(printLabelsDirectory, Clean(itemCode));
            var sampleDirectory = Path.Combine(root, "sample");

            Directory.CreateDirectory(sampleDirectory);

            return sampleDirectory;
        }
    }

}
