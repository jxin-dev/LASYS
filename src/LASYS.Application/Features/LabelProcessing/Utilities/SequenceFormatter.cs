namespace LASYS.Application.Features.LabelProcessing.Utilities
{
    public static class SequenceFormatter
    {
        public static string Format(int value, int padding) => value.ToString($"D{padding}");
    }
}
