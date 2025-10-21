using System.Drawing.Drawing2D;

namespace LASYS.UIControls.Controls
{
    public class ProfileAvatar : Control
    {
        public Image? ProfileImage { get; set; }
        public Color CircleColor { get; set; } = Color.FromArgb(82, 145, 192);

        public ProfileAvatar(Image? image = null)
        {
            ProfileImage = image;
            Size = new Size(64, 64);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Use high-quality rendering
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, Width - 1, Height - 1);

                // Fill circle background
                using (SolidBrush backBrush = new SolidBrush(CircleColor))
                {
                    e.Graphics.FillPath(backBrush, path);
                }

                // Draw profile image smoothly
                if (ProfileImage != null)
                {
                    // Clip the graphics region to a circle
                    e.Graphics.SetClip(path);
                    e.Graphics.DrawImage(ProfileImage, new Rectangle(0, 0, Width, Height));
                    e.Graphics.ResetClip();
                }

                // Optional border (white, clean edge)
                using (Pen pen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
                //base.OnPaint(e);
                //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                //using (GraphicsPath path = new GraphicsPath())
                //{
                //    path.AddEllipse(0, 0, Width - 1, Height - 1);

                //    using (SolidBrush backBrush = new SolidBrush(CircleColor))
                //    {
                //        e.Graphics.FillPath(backBrush, path);
                //    }

                //    if (ProfileImage != null)
                //    {
                //        using (TextureBrush imgBrush = new TextureBrush(ProfileImage))
                //        {
                //            imgBrush.ScaleTransform(
                //                (float)Width / ProfileImage.Width,
                //                (float)Height / ProfileImage.Height);
                //            e.Graphics.FillPath(imgBrush, path);
                //        }
                //    }

                //    using (Pen pen = new Pen(Color.White, 2))
                //    {
                //        e.Graphics.DrawPath(pen, path);
                //    }
                //}
            }
        }
    }
}
