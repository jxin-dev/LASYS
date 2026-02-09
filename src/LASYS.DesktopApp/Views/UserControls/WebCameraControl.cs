using LASYS.Camera.Interfaces;
using LASYS.Camera.Models;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;

namespace LASYS.DesktopApp.Views.UserControls
{
    public partial class WebCameraControl : UserControl, IWebCameraView
    {
        //private readonly ICameraConfig _cameraConfig;
        //private readonly ICameraService _cameraService;
        //private readonly ICameraEnumerator _cameraEnumerator;
        //private CancellationTokenSource? _previewCts;

        //public event Action? ConfigurationSaved;

        private readonly LoadingLabel _loadingLabel;
        private bool _isPreviewing;


        public event EventHandler? CameraPreviewStateChanged;
        public event EventHandler? CameraConfigurationSaved;

        public CameraInfo? SelectedCamera => cbxCameras.SelectedItem as CameraInfo;
        public WebCameraControl()
        {
            //_cameraConfig = cameraConfig;
            //_cameraEnumerator = cameraEnumerator;
            //_cameraService = cameraService;

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

            //Load += WebCameraControl_Load;

            btnPreview.Click += (sender, e) => CameraPreviewStateChanged?.Invoke(this, EventArgs.Empty);
            btnSave.Click += (sender, e) => CameraConfigurationSaved?.Invoke(this, EventArgs.Empty);
            btnSave.Visible = false;

        }

     

        private void BtnPreview_Click(object? sender, EventArgs e)
        {
            //if (!_isPreviewing)
            //{
            //    if (cbxCameras.SelectedItem is not CameraInfo selectedCamera)
            //    {
            //        MessageBox.Show(
            //            "Please select a camera.",
            //            "Camera Selection",
            //            MessageBoxButtons.OK,
            //            MessageBoxIcon.Exclamation);
            //        return;
            //    }

            //    _isPreviewing = true;
            //    cbxCameras.Enabled = false;
            //    btnPreview.Text = "Stop";
            //    btnPreview.Enabled = false;
            //    btnSave.Visible = false;

            //    _loadingLabel.Start();

            //    _previewCts = new CancellationTokenSource();
            //    Task.Run(() => _cameraService.PreviewCameraAsync(selectedCamera, (mat, bitmap) =>
            //    {
            //        if (picCameraPreview.InvokeRequired)
            //        {
            //            picCameraPreview.Invoke(() =>
            //            {
            //                picCameraPreview.Image?.Dispose();
            //                picCameraPreview.Image = bitmap;
            //            });
            //        }
            //        else
            //        {
            //            picCameraPreview.Image?.Dispose();
            //            picCameraPreview.Image = bitmap;
            //        }
            //    },
            //   () => new System.Drawing.Size(640, 480),
            //   _previewCts.Token));


            //    btnPreview.Enabled = true;
            //    btnSave.Visible = true;
            //    _loadingLabel.Stop();
            //}
            //else
            //{
            //    _previewCts?.Cancel();
            //    picCameraPreview.Image?.Dispose();
            //    picCameraPreview.Image = null;

            //    _isPreviewing = false;
            //    cbxCameras.Enabled = true;
            //    btnPreview.Text = "Preview";
            //    btnSave.Visible = false;

            //    //_cameraService.StopPreview(picCameraPreview);
            //}
        }

        //private void WebCameraControl_Load(object? sender, EventArgs e)
        //{
        //    this.BeginInvoke(async () =>
        //    {
        //        var cameras = _cameraEnumerator.GetCameras();
        //        cbxCameras.DataSource = cameras;
        //        cbxCameras.DisplayMember = "Name";
        //        cbxCameras.ValueMember = "Index";


        //        //await _deviceConfigService.LoadAsync();
        //    });
        //}

        public void SetPreviewButtonEnabled(bool isEnabled)
        {
            throw new NotImplementedException();
        }

        public void SetSaveButtonVisibility(bool visible)
        {
            throw new NotImplementedException();
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
    }
}
