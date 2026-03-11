using LASYS.Application.Common.Enums;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IErrorView
    {
        void InvokeOnUI(Action action);
        void CloseError();
        event EventHandler<OperatorDecision> DecisionRequested;

    }
}
