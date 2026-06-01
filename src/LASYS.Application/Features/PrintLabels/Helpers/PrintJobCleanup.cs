namespace LASYS.Application.Features.PrintLabels.Helpers
{
    public static class PrintJobCleanup
    {
        private const string LogsFolderName = "logs";
        private const string CleanupFolderName = "cleanup";
        public static Task<int> DeleteOldFilesByMonthsAsync(int monthsToKeep = 3, CancellationToken cancellationToken = default)
        {
            DateTime cutoffDate = DateTime.Now.AddMonths(-monthsToKeep);

            return DeleteOldFilesAsync(cutoffDate, cancellationToken);
        }

        public static Task<int> DeleteOldFilesByDaysAsync(int daysToKeep, CancellationToken cancellationToken = default)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

            return DeleteOldFilesAsync(cutoffDate, cancellationToken);
        }

        public static Task<int> DeleteOldFilesByHoursAsync(TimeSpan timeToKeep, CancellationToken cancellationToken = default)
        {
            DateTime cutoffDate = DateTime.Now.Subtract(timeToKeep);

            return DeleteOldFilesAsync(cutoffDate, cancellationToken);
        }

        private static async Task<int> DeleteOldFilesAsync(DateTime cutoffDate, CancellationToken cancellationToken)
        {
            string rootFolder = NiceLabelFilePathBuilder.GetPrintJobsDirectory();

            if (!Directory.Exists(rootFolder))
                return 0;

            return await Task.Run(() =>
            {
                int deletedFileCount = 0;
                string logDirectory = GetPrintJobCleanupLogDirectory();
                string logFile = Path.Combine(logDirectory, $"cleanup_{DateTime.Now:yyyyMMdd}.log");

                List<string> logEntries = new();
                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cleanup started. Cutoff: {cutoffDate:yyyy-MM-dd HH:mm:ss}");
                // Delete old files
                foreach (string filePath in Directory.EnumerateFiles(rootFolder, "*", SearchOption.AllDirectories))
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
                    catch (Exception ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FILE DELETE FAILED: {filePath} | {ex.Message}");
                    }
                }
                // Delete empty directories
                foreach (string directory in Directory
                             .EnumerateDirectories(rootFolder, "*", SearchOption.AllDirectories)
                             .OrderByDescending(x => x.Length))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory);
                            logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DIRECTORY DELETED: {directory}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] DIRECTORY DELETE FAILED: {directory} | {ex.Message}");
                    }
                }
                logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cleanup finished. Deleted Files: {deletedFileCount}");
                File.AppendAllLines(logFile, logEntries);

                return deletedFileCount;
            }, cancellationToken);
        }

        private static string GetPrintJobCleanupLogDirectory()
        {
            string root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogsFolderName, CleanupFolderName);
            Directory.CreateDirectory(root);

            return root;
        }
    }
}
