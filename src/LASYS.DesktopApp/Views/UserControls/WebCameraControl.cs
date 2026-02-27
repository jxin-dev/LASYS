using LASYS.Application.Contracts;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WebCameraControl : UserControl, IWebCameraView
    {
        private readonly LoadingLabel? _loadingLabel;
        public event EventHandler<CameraSavedEventArgs>? CameraConfigurationSaved;
        public event EventHandler<CameraSelectedEventArgs>? CameraPreviewStateChanged;

        public CameraInfo? SelectedCamera => cbxCameras.SelectedItem as CameraInfo;
        public WebCameraControl()
        {
            InitializeComponent();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cbxCameras.SelectedIndexChanged += (sender, e) =>
            {
                var selectedCamera = cbxCameras.SelectedItem as CameraInfo;
                if (selectedCamera == null)
                {
                    MessageBox.Show(
                        "Please select a camera.",
                        "Camera Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                CameraPreviewStateChanged?.Invoke(this, new CameraSelectedEventArgs(selectedCamera.Index));

            };
            btnSave.Click += (sender, e) =>
            {
                var selectedCamera = cbxCameras.SelectedItem as CameraInfo;
                if (selectedCamera == null)
                {
                    MessageBox.Show(
                        "Please select a camera.",
                        "Camera Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
                if (cbxResolutions.SelectedItem == null)
                {
                    MessageBox.Show("Please select a resolution.", "Camera Configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                CameraConfigurationSaved?.Invoke(this, new CameraSavedEventArgs(selectedCamera.Index, selectedCamera.Name, cbxResolutions.Text));
            };
        }

        public void SetCameraList(IEnumerable<CameraInfo> cameras)
        {
            cbxCameras.DataSource = cameras.ToList();
            cbxCameras.DisplayMember = "Name";
            cbxCameras.ValueMember = "Index";
        }

        public void InvokeOnUI(Action action)
        {
            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        public void ShowMessage(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        public void SetCameraResolutions(IEnumerable<string> resolution)
        {
            cbxResolutions.Items.Clear();
            cbxResolutions.Items.AddRange(resolution.ToArray());
        }

        public void DisplayFrame(Bitmap bitmap)
        {
            picCameraPreview.Image = bitmap;
        }

        public void ShowMessage(string message)
        {
            throw new NotImplementedException();
        }

        public bool AskRestartConfirmation(string message, string title = "Restart Required")
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }
    }
}
