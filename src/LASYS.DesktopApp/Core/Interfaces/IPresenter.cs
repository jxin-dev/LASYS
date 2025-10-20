namespace LASYS.DesktopApp.Core.Interfaces
{
    public interface IPresenter<in TView> where TView : IView
    {
        void AttachView(TView view);
    }
}
