namespace LASYS.Application.Features.BatchPrinting.Helpers
{
    public static class PrintFileNameBuilder
    {
        public static string Build(string itemCode, string lotNo, string sequence)
        {
            return $"{Clean(itemCode)}_LOT-{Clean(lotNo)}_{sequence}";
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
