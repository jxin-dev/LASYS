using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class OcrItemLookupPresenter
    {
        private readonly IOcrItemLookupView _view;
        public OcrItemLookupPresenter(IOcrItemLookupView view)
        {
            _view = view;
        }

        public void Show()
        {
           _view.ShowDialog();
        }
    }
}
