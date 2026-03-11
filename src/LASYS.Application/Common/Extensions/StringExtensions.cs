namespace LASYS.Application.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Ellipsis(this string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
                return value;

            return value[..maxLength] + "...";
        }
    }
}
