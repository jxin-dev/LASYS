namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface IErrorView
    {
        //Events for Error Message
        event EventHandler RetryRequested;
        event EventHandler SkipRequested;
        event EventHandler StopBatchRequested;

        void CloseError();
    }
}
