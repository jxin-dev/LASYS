using System.IO.Compression;
using LASYS.Application.Common.Enums;

namespace LASYS.Application.Features.PrintLabels.Helpers
{
    public static class NiceLabelFilePathBuilder
    {
        private const string PrintJobsFolderName = "labelfiles";
        private const string TemplatesFolderName = "templates";
        public static async Task CreateFileAsync(string filePath, byte[] compressedBytes, CancellationToken cancellationToken = default)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Decompress the GZip bytes before writing
            byte[] decompressedBytes = Decompress(compressedBytes);
            await File.WriteAllBytesAsync(
                filePath,
                decompressedBytes,
                cancellationToken);
        }

        private static byte[] Decompress(byte[] compressedData)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gzipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        public static async Task CopyFileAsync(string sourceFilePath, string destinationFilePath, bool overwrite = true, CancellationToken cancellationToken = default)
        {
            string? directory = Path.GetDirectoryName(destinationFilePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream sourceStream = new(
                sourceFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            await using FileStream destinationStream = new(
                destinationFilePath,
                overwrite ? FileMode.Create : FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        }


        public static string Build(string itemCode, uint revisionNumber, BoxType boxType)
        {
            string itemFolder = GetPrintLabelsDirectory(itemCode);

            string templateDirectory = Path.Combine(itemFolder, TemplatesFolderName);

            string fileName =
                $"{Clean(itemCode)}_R{revisionNumber}_{boxType}.lbl";

            return Path.Combine(templateDirectory, fileName);
        }

        private static string GetPrintLabelsDirectory(string itemCode)
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PrintJobsFolderName, Clean(itemCode));
            Directory.CreateDirectory(root);
            return root;
        }

        public static string GetPrintJobsDirectory()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PrintJobsFolderName);
            Directory.CreateDirectory(root);
            return root;
        }


        private static string Clean(string value)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '-');
            }

            return value.Trim();
        }
    }
}
