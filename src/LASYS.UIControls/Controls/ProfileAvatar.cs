using System.Drawing.Drawing2D;

namespace LASYS.UIControls.Controls
{
    public class ProfileAvatar : Control
    {
        private Image? _resizedProfileImage;

        private Image? _profileImage;
        public Image? ProfileImage
        {
            get => _profileImage;
            set
            {
                _profileImage = value;
                _resizedProfileImage?.Dispose();
                _resizedProfileImage = value != null ? new Bitmap(value, Width, Height) : null;
                Invalidate(); // trigger repaint
            }
        }

        public Color CircleColor { get; set; } = Color.FromArgb(82, 145, 192);

        public ProfileAvatar(Image? image = null)
        {
            ProfileImage = image;
            Size = new Size(64, 64);
            DoubleBuffered = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            // Rescale image if control size changes
            if (ProfileImage != null)
            {
                _resizedProfileImage?.Dispose();
                _resizedProfileImage = new Bitmap(ProfileImage, Width, Height);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

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

                // Draw profile image with TextureBrush
                if (_resizedProfileImage != null)
                {
                    using (TextureBrush brush = new TextureBrush(_resizedProfileImage))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }

                // Draw white circular border
                using (Pen pen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _resizedProfileImage?.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
