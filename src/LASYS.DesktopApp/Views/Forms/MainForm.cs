using System.Windows.Forms;
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
        public event EventHandler? VisionSettingsRequested;
        public event EventHandler? PrinterManagementRequested;
        public event EventHandler? BarcodeDeviceSetupRequested;
        public event EventHandler? FormClosingRequested;
        public event EventHandler? LogoutRequested;

        private NavItem? _workOrders;
        private NavItem? _visionSettings;
        private NavItem? _printerManagement;
        private NavItem? _barcode;
        private NavItem? _logout;
        public MainForm()
        {
            InitializeComponent();

            // Initialize layout
            _sideNav = new SideNavigation();
            //_sideNav.SetProfile("Guest User", @"C:\Users\ITC - JAYSON OLICIA\Downloads\cartoon-1890438_1280.jpg");

            try
            {
                _sideNav.SetProfile("Guest User", "Section Name", @"C:\Users\ITC - JAYSON OLICIA\Downloads\cartoon-1890438_1280.jpg");
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
            _workOrders = new NavItem { Text = "Work Orders" };
            var deviceSetup = new NavItem { Text = "Device Settings" };
            _logout = new NavItem { Text = "Log Out" };
            // Define sub-items
            _visionSettings = new NavItem { Text = "Vision Settings" };
            _printerManagement = new NavItem { Text = "Printer Management" };
            _barcode = new NavItem { Text = "Barcode Scanner" };
            // Add sub-items
            deviceSetup.SubItems.Add(_visionSettings);
            deviceSetup.SubItems.Add(_printerManagement);
            deviceSetup.SubItems.Add(_barcode);

            // Add to navigation
            _sideNav.AddItem(_workOrders);
            _sideNav.AddItem(deviceSetup);
            _sideNav.AddItem(_logout);


            _workOrders.Clicked += delegate { WorkOrderRequested?.Invoke(this, EventArgs.Empty); };
            _visionSettings.Clicked += (_, _) => VisionSettingsRequested?.Invoke(this, EventArgs.Empty);
            _printerManagement.Clicked += (_, _) => PrinterManagementRequested?.Invoke(this, EventArgs.Empty);
            _barcode.Clicked += (_, _) => BarcodeDeviceSetupRequested?.Invoke(this, EventArgs.Empty);
            _logout.Clicked += (_, _) => LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //WorkOrderRequested?.Invoke(this, EventArgs.Empty);
            _sideNav.SetActiveItem(_workOrders);
            WorkOrderRequested?.Invoke(this, EventArgs.Empty);
        }
        public void CloseView() => Close();

        private Dictionary<Type, UserControl> _views = new Dictionary<Type, UserControl>();
        public void LoadView(UserControl control, bool cache = true)
        {
            var type = control.GetType();

            _contentPanel.SuspendLayout();

            if (_contentPanel.Controls.Count > 0 && _contentPanel.Controls[0].GetType() == type)
            {
                _contentPanel.ResumeLayout();
                return;
            }

            if (cache)
            {
                if (!_views.ContainsKey(type))
                    _views[type] = control;

                control = _views[type];
            }

            _contentPanel.Controls.Clear();

            control.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(control);

            _contentPanel.ResumeLayout();
            _contentPanel.Refresh();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            FormClosingRequested?.Invoke(this, EventArgs.Empty);
            base.OnFormClosing(e);
        }

        public void ShowUserInfo(string fullName, string sectionName, string? imagePath)
        {
            _sideNav.SetProfile(fullName, sectionName, imagePath);
        }

        public void ShowNavigationBlocked(string message)
        {
            MessageBox.Show(message, "Navigation Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private NavItem? _lastSelected;
        public NavItem WorkOrdersNavItem => _workOrders!;
        public NavItem VisionSettingsNavItem => _visionSettings!;
        public NavItem PrinterManagementNavItem => _printerManagement!;
        public NavItem BarcodeNavItem => _barcode!;

        public void SetActiveNavigation(NavItem? item)
        {
            _lastSelected = item;
            _sideNav.SetActiveItem(item);
        }
        public void RestorePreviousNavigation()
        {
            _sideNav.SetActiveItem(_lastSelected);
        }
    }
}
