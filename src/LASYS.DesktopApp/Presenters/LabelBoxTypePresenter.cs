using LASYS.Application.Common.Enums;
using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Presenters
{
    public class LabelBoxTypePresenter
    {
        private readonly ILabelBoxTypeView _view;
        public LabelBoxTypePresenter(ILabelBoxTypeView view)
        {
            _view = view;
        }
        public BoxType? Show(IReadOnlyCollection<BoxType>? availableBoxTypes)
        {
            if (availableBoxTypes == null || availableBoxTypes.Count == 0)
                return null;

            _view.RenderButtons(availableBoxTypes);

            return _view.ShowDialog() == DialogResult.OK
                ? _view.SelectedType
                : null;
        }
    }
}
