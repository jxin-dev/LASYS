using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IErrorView
    {
        void InvokeOnUI(Action action);
        void CloseError();
        event EventHandler<StepResult> DecisionRequested;

    }
}
