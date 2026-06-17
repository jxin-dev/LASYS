using System.Windows.Forms;
using LASYS.Application.Common.Enums;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class CameraPreviewControl : UserControl, ICameraPreviewView
    {
        public CameraPreviewControl()
        {
            InitializeComponent();
            picPreview.SizeMode = PictureBoxSizeMode.StretchImage;
            BorderStyle = BorderStyle.FixedSingle;
            Disposed += (_, _) =>
            {
                picPreview?.Image?.Dispose();
            };
        }

        public UserControl View => this;

        public void DisplayFrame(Bitmap frame)
        {
            if (picPreview == null)
                return;

            lblNoCameraDisplayText.Visible = false;
            var old = picPreview.Image;

            picPreview.Image = (Bitmap)frame.Clone();

            old?.Dispose();
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
