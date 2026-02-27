using System.Drawing;

namespace LASYS.Application.Contracts
{
    public class NormalizedRect
    {
        public float X; // 0 to 1
        public float Y;
        public float Width;
        public float Height;

        public Rectangle ToAbsolute(Size size)
        {
            return new Rectangle(
                (int)(X * size.Width),
                (int)(Y * size.Height),
                (int)(Width * size.Width),
                (int)(Height * size.Height)
            );
        }
        public static NormalizedRect FromAbsolute(Rectangle rect, Size size)
        {
            return new NormalizedRect
            {
                X = (float)rect.X / size.Width,
                Y = (float)rect.Y / size.Height,
                Width = (float)rect.Width / size.Width,
                Height = (float)rect.Height / size.Height
            };
        }
    }
}
