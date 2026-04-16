using LASYS.DesktopApp.Views.Forms;
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
        public LabelBoxType? Show(
        bool hasCaseLabel,
        bool hasUnitBox,
        bool hasAdditionalUnitBox,
        bool hasOuterUnitBox,
        bool hasCartonBox,
        bool hasOuterCartonBox,
        bool hasAdditionalCartonBox)
        {
            var types = BuildTypes(
                hasCaseLabel,
                hasUnitBox,
                hasAdditionalUnitBox,
                hasOuterUnitBox,
                hasCartonBox,
                hasOuterCartonBox,
                hasAdditionalCartonBox);

            if (!types.Any())
                return null;

            _view.RenderButtons(types);

            return _view.ShowDialog() == DialogResult.OK
                ? _view.SelectedType
                : null;
        }

        private IEnumerable<LabelBoxType> BuildTypes(
            bool hasCaseLabel,
            bool hasUnitBox,
            bool hasAdditionalUnitBox,
            bool hasOuterUnitBox,
            bool hasCartonBox,
            bool hasOuterCartonBox,
            bool hasAdditionalCartonBox)
        {
            return new[]
            {
            (hasCaseLabel, LabelBoxType.CaseLabel),
            (hasUnitBox, LabelBoxType.UnitBox),
            (hasAdditionalUnitBox, LabelBoxType.AdditionalUnitBox),
            (hasOuterUnitBox, LabelBoxType.OuterUnitBox),
            (hasCartonBox, LabelBoxType.CartonBox),
            (hasOuterCartonBox, LabelBoxType.OuterCartonBox),
            (hasAdditionalCartonBox, LabelBoxType.AdditionalCartonBox)
        }
            .Where(x => x.Item1)
            .Select(x => x.Item2);
        }
    }
}
