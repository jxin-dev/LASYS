using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.UIControls.Controls;
using LASYS.UIControls.Models;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class MainForm : Form, IMainView
    {
        private readonly SideNavigation _sideNav;
        private readonly Panel _contentPanel;
        public MainForm()
        {
            InitializeComponent();
            // Initialize layout
            _sideNav = new SideNavigation();
            _sideNav.SetProfile("Guest User", @"C:\Users\ITC - JAYSON OLICIA\Downloads\cartoon-1890438_1280.jpg");
            _contentPanel = new Panel
            {
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
            var deviceSetup = new NavItem { Text = "Device Setup" };
            var logOut = new NavItem { Text = "Log Out" };
            // Define sub-items
            deviceSetup.SubItems.Add(new NavItem { Text = "Web Camera" });
            deviceSetup.SubItems.Add(new NavItem { Text = "OCR Calibration" });
            deviceSetup.SubItems.Add(new NavItem { Text = "SATO Printer" });
            deviceSetup.SubItems.Add(new NavItem { Text = "Barcode Scanner" });
            // Add to navigation
            _sideNav.AddItem(workOrders);
            _sideNav.AddItem(deviceSetup);
            _sideNav.AddItem(logOut);


            // Wire events
            workOrders.Clicked += (_, _) => LoadView(new WorkOrdersControl());
            deviceSetup.SubItems[0].Clicked += (_, _) => LoadView(new WebCameraControl());
            //deviceSetup.SubItems[1].Clicked += (_, _) => LoadView(new OcrCalibrationControl());
            //deviceSetup.SubItems[2].Clicked += (_, _) => LoadView(new SatoPrinterControl());
            //deviceSetup.SubItems[3].Clicked += (_, _) => LoadView(new BarcodeScannerControl());
        }
        private void LoadView(UserControl control)
        {
            _contentPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(control);
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Navigation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CloseView();
        }
        public void CloseView() => Application.Exit();

        public void HideView() => Hide();

        public void ShowView() => Show();
    }
}
