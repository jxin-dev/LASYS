using LASYS.DesktopApp.Views.Interfaces;
using LASYS.UIControls.Controls;
using LASYS.UIControls.Models;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class MainForm : Form, IMainView
    {
        private readonly SideNavigation _sideNav;
        private readonly DoubleBufferedPanel _contentPanel;

        public event EventHandler? WorkOrderRequested;
        public event EventHandler? WebCameraConfigurationRequested;
        public event EventHandler? OCRCalibrationRequested;
        public event EventHandler? PrinterManagementRequested;
        public event EventHandler? BarcodeDeviceSetupRequested;

        public MainForm()
        {
            InitializeComponent();

            // Initialize layout
            _sideNav = new SideNavigation();
            //_sideNav.SetProfile("Guest User", @"C:\Users\ITC - JAYSON OLICIA\Downloads\cartoon-1890438_1280.jpg");

            try
            {
                _sideNav.SetProfile("Guest User", @"C:\Users\ITC - JAYSON OLICIA\Downloads\cartoon-1890438_1280.jpg");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Failed to load profile image: {ex.Message}");
            }

            _contentPanel = new DoubleBufferedPanel
            {
                Name = "contentPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(221, 221, 221)
            };

            Controls.Add(_contentPanel);
            Controls.Add(_sideNav);

            // Setup navigation items
            SetupNavigation();


        }
        private void SetupNavigation()
        {
            // Define main menu items
            var workOrders = new NavItem { Text = "Work Orders" };
            var deviceSetup = new NavItem { Text = "Settings" };
            var logOut = new NavItem { Text = "Log Out" };
            // Define sub-items
            deviceSetup.SubItems.Add(new NavItem { Text = "Camera Configuration" });
            deviceSetup.SubItems.Add(new NavItem { Text = "OCR Calibration" });
            deviceSetup.SubItems.Add(new NavItem { Text = "Printer Management" });
            deviceSetup.SubItems.Add(new NavItem { Text = "Barcode Device Setup" });
            // Add to navigation
            _sideNav.AddItem(workOrders);
            _sideNav.AddItem(deviceSetup);
            _sideNav.AddItem(logOut);


            workOrders.Clicked += (_, _) => WorkOrderRequested?.Invoke(this, EventArgs.Empty);

            deviceSetup.SubItems[0].Clicked += (_, _) => WebCameraConfigurationRequested?.Invoke(this, EventArgs.Empty);

            deviceSetup.SubItems[1].Clicked += (_, _) => OCRCalibrationRequested?.Invoke(this, EventArgs.Empty);

            deviceSetup.SubItems[2].Clicked += (_, _) => PrinterManagementRequested?.Invoke(this, EventArgs.Empty);

            deviceSetup.SubItems[3].Clicked += (_, _) => BarcodeDeviceSetupRequested?.Invoke(this, EventArgs.Empty);
        }


        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Navigation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WorkOrderRequested?.Invoke(this, EventArgs.Empty);

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CloseView();
        }
        public void CloseView() => Close();//System.Windows.Forms.Application.Exit();

        public void HideView() => Hide();

        public void ShowView() => Show();

        private Dictionary<Type, UserControl> _views = new Dictionary<Type, UserControl>();
        public void LoadView(UserControl control)
        {
            var type = control.GetType();
            if (_contentPanel.Controls.Count > 0 && _contentPanel.Controls[0].GetType() == type) return;

            _contentPanel.SuspendLayout();

            if (!_views.ContainsKey(type))
                _views[type] = control;

            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(_views[type]);
            _views[type].Dock = DockStyle.Fill;

            _contentPanel.ResumeLayout();
            _contentPanel.Refresh();


            //foreach (Control c in _contentPanel.Controls)
            //{
            //    if (c.GetType() == control.GetType())
            //    {
            //        // Already showing, do nothing
            //        return;
            //    }
            //}
            //_contentPanel.SuspendLayout();
            //_contentPanel.Controls.Clear();
            //control.Dock = DockStyle.Fill;
            //_contentPanel.Controls.Add(control);
            //_contentPanel.ResumeLayout();
            //_contentPanel.Refresh();
        }
    }
}
