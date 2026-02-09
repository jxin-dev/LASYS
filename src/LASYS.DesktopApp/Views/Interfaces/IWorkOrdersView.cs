using LASYS.DesktopApp.Events;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IWorkOrdersView
    {
        event EventHandler<LabelPrintingRequestedEventArgs> LabelPrintingRequested;
    }
}
