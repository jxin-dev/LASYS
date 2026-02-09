using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class ErrorPresenter
    {
        private readonly IErrorView _view;

        public ErrorPresenter(IErrorView view)
        {
            _view = view;
            //Events for error message
            _view.RetryRequested += OnRetryRequested;
            _view.SkipRequested += OnSkipRequested;
            _view.StopBatchRequested += OnStopBatchRequested;
        }

        private void OnStopBatchRequested(object? sender, EventArgs e)
        {
            _view.CloseError();
        }

        private void OnSkipRequested(object? sender, EventArgs e)
        {
            _view.CloseError();
        }

        private void OnRetryRequested(object? sender, EventArgs e)
        {
            _view.CloseError();
        }
    }
}
