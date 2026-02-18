namespace LASYS.DesktopApp.DTOs
{
    public record PrinterDto
    {
        public bool Selected { get; set; }
        public string Name { get; init; }
        public string Series { get; init; }
        public string Status { get; init; }
    }
}
