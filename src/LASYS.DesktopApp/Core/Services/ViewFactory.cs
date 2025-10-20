using LASYS.DesktopApp.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace LASYS.DesktopApp.Core.Services
{
    public class ViewFactory : IViewFactory
    {
        private readonly IServiceProvider _provider;
        public ViewFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public TView Create<TView, TPresenter>()
            where TView : class, IView
            where TPresenter : class, IPresenter<TView>
        {
            var view = _provider.GetRequiredService<TView>();
            var presenter = _provider.GetRequiredService<TPresenter>();
            presenter.AttachView(view);
            return view;
        }
    }
}
