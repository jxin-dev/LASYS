namespace LASYS.Camera.Models
{
    public class Resolution
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string AspectRatio { get; set; }
        public string Notes { get; set; }

        public override string ToString() => $"{Width}x{Height} ({AspectRatio})";


    }
}
