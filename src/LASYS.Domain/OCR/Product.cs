namespace LASYS.Domain.OCR
{
    public class Product
    {
        public string ItemCode { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new();
    }

}
