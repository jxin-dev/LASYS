using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.UserControls
{

    public partial class LabelTemplatePreviewControl : UserControl, ILabelTemplatePreviewView
    {
        private Point _lastMousePosition;
        private bool _isDragging;

        private float _zoom = 1.0f;
        private Size _originalImageSize;
        private bool _isFirstImage = true;

        public LabelTemplatePreviewControl()
        {
            InitializeComponent();

            BorderStyle = BorderStyle.FixedSingle;

            pnlPreview.Dock = DockStyle.Fill;
            pnlPreview.AutoScroll = false;

            picPreview.Location = Point.Empty;
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;

            picPreview.MouseDown += PicPreview_MouseDown;
            picPreview.MouseMove += PicPreview_MouseMove;
            picPreview.MouseUp += PicPreview_MouseUp;

            picPreview.MouseWheel += PicPreview_MouseWheel;
            picPreview.MouseEnter += (_, _) => picPreview.Focus();

            picPreview.DoubleClick += (_, _) => ResetZoom();

            Disposed += (_, _) =>
            {
                picPreview?.Image?.Dispose();
            };
        }

        public UserControl View => this;

        public void DisplayTemplate(Bitmap frame)
        {
            if (picPreview == null)
                return;

            var old = picPreview.Image;

            picPreview.Image = (Bitmap)frame.Clone();

            old?.Dispose();

            _originalImageSize = frame.Size;
            if (_isFirstImage)
            {
                FitImageToPanel();
                _isFirstImage = false;
            }
        }
        private void FitImageToPanel()
        {
            if (picPreview.Image == null)
                return;

            float ratioX = (float)pnlPreview.ClientSize.Width / _originalImageSize.Width;
            float ratioY = (float)pnlPreview.ClientSize.Height / _originalImageSize.Height;

            _zoom = Math.Min(ratioX, ratioY);

            picPreview.Size = new Size(
                (int)(_originalImageSize.Width * _zoom),
                (int)(_originalImageSize.Height * _zoom));

            CenterImage();
        }
        private void ResetZoom()
        {
            FitImageToPanel();
        }

        private void PicPreview_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (picPreview.Image == null)
                return;

            if (e.Delta > 0)
                _zoom *= 1.1f;
            else
                _zoom /= 1.1f;

            _zoom = Math.Clamp(_zoom, 0.25f, 10f);

            float relativeX = (float)e.X / picPreview.Width;
            float relativeY = (float)e.Y / picPreview.Height;

            int oldWidth = picPreview.Width;
            int oldHeight = picPreview.Height;

            picPreview.Size = new Size(
                (int)(_originalImageSize.Width * _zoom),
                (int)(_originalImageSize.Height * _zoom));

            picPreview.Left -= (int)((picPreview.Width - oldWidth) * relativeX);
            picPreview.Top -= (int)((picPreview.Height - oldHeight) * relativeY);
        }

        private void PicPreview_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _isDragging = true;
            _lastMousePosition = e.Location;

            picPreview.Cursor = Cursors.SizeAll;
        }

        private void PicPreview_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isDragging)
                return;

            int dx = e.X - _lastMousePosition.X;
            int dy = e.Y - _lastMousePosition.Y;

            picPreview.Left += dx;
            picPreview.Top += dy;
        }

        private void PicPreview_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
            picPreview.Cursor = Cursors.Default;
        }

        private void CenterImage()
        {
            picPreview.Location = new Point(
                (pnlPreview.ClientSize.Width - picPreview.Width) / 2,
                (pnlPreview.ClientSize.Height - picPreview.Height) / 2);
        }

        public void InvokeOnUI(Action action)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }
    }
}

