using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class CameraPreviewControl : UserControl, ICameraPreviewView
    {
        private Rectangle? _ocrRegion;
        public CameraPreviewControl()
        {
            InitializeComponent();
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            BorderStyle = BorderStyle.FixedSingle;
            Disposed += (_, _) =>
            {
                picPreview?.Image?.Dispose();
            };
            picPreview.Paint += PicPreview_Paint;
        }

        private void PicPreview_Paint(object? sender, PaintEventArgs e)
        {
            if (_ocrRegion is not Rectangle region)
                return;

            using var pen = new Pen(Color.LimeGreen, 2);

            e.Graphics.DrawRectangle(pen, region);
        }

        public UserControl View => this;

        public Size PictureBoxSize => picPreview.ClientSize; //If you change to PictureBoxSizeMode.Zoom

        public void DisplayFrame(Bitmap frame)
        {
            if (picPreview == null)
                return;

            lblNoCameraDisplayText.Visible = false;
            var old = picPreview.Image;

            picPreview.Image = (Bitmap)frame.Clone();

            old?.Dispose();
        }

        public void HideOcrRegion()
        {
            _ocrRegion = null;
            picPreview.Invalidate();
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

        public void ShowOcrRegion(Rectangle region)
        {
            _ocrRegion = region;
            picPreview.Invalidate();
        }
    }
}
