using LASYS.Cleanup.Enums;
using LASYS.Cleanup.Models;

namespace LASYS.Cleanup.Features.Cleanup
{
    public sealed class CleanupRunner : ICleanupRunner
    {
        public async Task<int> RunAsync(ScheduleSettings settings, CancellationToken cancellationToken = default)
        {
            if (!settings.Enabled)
                return 0;
            if (!Directory.Exists(settings.CleanupFolder))
            {
                throw new DirectoryNotFoundException(
                    $"Cleanup folder does not exist: " +
                    $"{settings.CleanupFolder}");
            }
            DateTime cutoffDate = settings.RetentionUnit switch
            {
                RetentionUnit.Hours =>
                    DateTime.Now.AddHours(
                        -settings.RetentionValue),

                RetentionUnit.Days =>
                    DateTime.Now.AddDays(
                        -settings.RetentionValue),

                RetentionUnit.Months =>
                    DateTime.Now.AddMonths(
                        -settings.RetentionValue),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(settings.RetentionUnit))
            };
            return await DeleteOldFilesAsync(
            settings.CleanupFolder,
            cutoffDate,
            cancellationToken);
        }
        private static async Task<int> DeleteOldFilesAsync(string folder, DateTime cutoffDate, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                int deletedFileCount = 0;

                // Prepare log
                string logDirectory = GetCleanupLogDirectory();
                string logFile = Path.Combine(logDirectory, $"cleanup_{DateTime.Now:yyyyMMdd}.log");
                List<string> logEntries = new();
                DateTime cleanupStart = DateTime.Now;
                logEntries.Add($"[{cleanupStart:yyyy-MM-dd HH:mm:ss}] ==================================================");
                logEntries.Add($"[{cleanupStart:yyyy-MM-dd HH:mm:ss}] Cleanup started.");
                logEntries.Add($"[{cleanupStart:yyyy-MM-dd HH:mm:ss}] Cleanup folder: {folder}");
                logEntries.Add($"[{cleanupStart:yyyy-MM-dd HH:mm:ss}] Cutoff date: {cutoffDate:yyyy-MM-dd HH:mm:ss}");
                // Total size BEFORE cleanup
                long totalSizeBefore = GetDirectorySize(folder, cancellationToken);
                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total size before cleanup: {FormatFileSize(totalSizeBefore)}");
                // Delete old files
                foreach (string filePath in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        FileInfo fileInfo = new(filePath);

                        if (fileInfo.LastWriteTime < cutoffDate)
                        {
                            fileInfo.Delete();
                            deletedFileCount++;
                            logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE DELETED: {filePath}");
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE DELETE FAILED - ACCESS DENIED: {filePath} | {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE DELETE FAILED - IO ERROR: {filePath} | {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE DELETE FAILED: {filePath} | {ex.Message}");
                    }
                }

                // Delete empty directories
                foreach (string directory in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories).OrderByDescending(x => x.Length))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory);
                        }
                    }
                    catch (Exception ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DIRECTORY DELETE FAILED: {directory} | {ex.Message}");
                    }
                }

                // Total size AFTER cleanup
                long totalSizeAfter = GetDirectorySize(folder, cancellationToken);
                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total size after cleanup: {FormatFileSize(totalSizeAfter)}");
                // Total space freed
                long totalSpaceFreed = totalSizeBefore - totalSizeAfter;
                if (totalSpaceFreed < 0)
                    totalSpaceFreed = 0;

                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total deleted size: {FormatFileSize(totalSpaceFreed)}");
                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Files deleted: {deletedFileCount}");

                DateTime cleanupFinished = DateTime.Now;
                TimeSpan duration = cleanupFinished - cleanupStart;
                logEntries.Add($"[{cleanupFinished:yyyy-MM-dd HH:mm:ss}] Cleanup finished.");
                logEntries.Add($"[{cleanupFinished:yyyy-MM-dd HH:mm:ss}] Duration: {duration}");
                logEntries.Add($"[{cleanupFinished:yyyy-MM-dd HH:mm:ss}] ==================================================");

                // Write log
                File.AppendAllLines(logFile, logEntries);

                return deletedFileCount;

            }, cancellationToken);
        }
        private static long GetDirectorySize(
           string folder,
           CancellationToken cancellationToken)
        {
            long totalSize = 0;

            foreach (string filePath in
                Directory.EnumerateFiles(
                    folder,
                    "*",
                    SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    FileInfo fileInfo =
                        new(filePath);

                    totalSize += fileInfo.Length;
                }
                catch
                {
                    // Ignore files that cannot be accessed.
                }
            }

            return totalSize;
        }
        private static string GetCleanupLogDirectory()
        {
            //string root = Path.Combine(
            //    Environment.GetFolderPath(
            //        Environment.SpecialFolder.LocalApplicationData),
            //    "InnovaThinkCorporation",
            //    "LASYS-Cleanup",
            //    "logs");

            string root = Path.Combine(AppContext.BaseDirectory, "logs");

            Directory.CreateDirectory(root);

            return root;
        }
        private static string FormatFileSize(
            long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024d:0.##} KB";

            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / 1024d / 1024d:0.##} MB";

            return $"{bytes / 1024d / 1024d / 1024d:0.##} GB";
        }
    }
}
