namespace LASYS.DesktopApp.Core.Interfaces
{
    public interface IViewFactory
    {
        TView Create<TView, TPresenter>()
          where TView : class, IView
          where TPresenter : class, IPresenter<TView>;
    }
}
