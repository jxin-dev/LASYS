using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class PrinterManagementPresenter
    {
        public UserControl View { get; }
        private readonly IPrinterManagementView _view;

        public PrinterManagementPresenter(IPrinterManagementView view)
        {
            _view = view;
            View = (UserControl)view;
        }
    }
}
