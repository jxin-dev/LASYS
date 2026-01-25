using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using LASYS.Camera.Services;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WebCameraControl : UserControl
    {
        private readonly ICameraService _cameraService;
        private readonly LoadingLabel _loadingLabel;

        private bool _isPreviewing;
        public WebCameraControl()
        {
            InitializeComponent();
            _loadingLabel = new LoadingLabel
            {
                BaseText = "Please wait",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(cbxCameras.Left + 5, cbxCameras.Bottom + 8),
                AutoSize = true,
                Visible = false // hidden by default
            };

            pnlContent.Controls.Add(_loadingLabel);

            _cameraService = new CameraService();
            Load += WebCameraControl_Load;

            btnPreview.Click += BtnPreview_Click;
            btnSave.Click += BtnSave_Click;
            btnSave.Visible = false;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {

        }

        private async void BtnPreview_Click(object? sender, EventArgs e)
        {
            if (!_isPreviewing)
            {
                if (cbxCameras.SelectedItem is not CameraDevice selectedCamera)
                {
                    MessageBox.Show(
                        "Please select a camera.",
                        "Camera Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                _isPreviewing = true;
                cbxCameras.Enabled = false;
                btnPreview.Text = "Stop";
                btnPreview.Enabled = false;
                btnSave.Visible = false;

                _loadingLabel.Start();

                _ = _cameraService.StartPreviewAsync(selectedCamera, picCameraPreview);

                // wait ONLY for first frame
                await _cameraService.PreviewStartedAsync;

                btnPreview.Enabled = true;
                btnSave.Visible = true;
                _loadingLabel.Stop();
            }
            else
            {
                _isPreviewing = false;
                cbxCameras.Enabled = true;
                btnPreview.Text = "Preview";
                btnSave.Visible = false;

                _cameraService.StopPreview(picCameraPreview);
            } 
        }

        private void WebCameraControl_Load(object? sender, EventArgs e)
        {
            var cameras = _cameraService.GetAvailableCameras();
            cbxCameras.DataSource = cameras;
            cbxCameras.DisplayMember = "Name";
            cbxCameras.ValueMember = "Index";
        }
    }
}
