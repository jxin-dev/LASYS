namespace LASYS.Application.Common.Utilities
{
    public static class SequenceFormatter
    {
        public static string Format(int value, int padding) => value.ToString($"D{padding}");
        public static string Format(long value, int padding) => value.ToString($"D{padding}");

    }
}
