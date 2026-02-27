namespace LASYS.Application.Contracts
{
    public class OCRConfig
    {
        public List<Product> Products { get; set; } = [];
    }
    public class Product
    {
        public string ItemCode { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new();
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }
}
