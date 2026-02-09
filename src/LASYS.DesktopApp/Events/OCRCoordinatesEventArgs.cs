namespace LASYS.DesktopApp.Events
{
    public sealed class OCRCoordinatesEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public int ImageWidth { get; }
        public int ImageHeight { get; }

        public OCRCoordinatesEventArgs(int x,
                                  int y,
                                  int width,
                                  int height,
                                  int imageWidth,
                                  int imageHeight)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
        }
    }

}
