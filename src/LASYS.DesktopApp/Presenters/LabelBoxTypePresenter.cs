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

        //private IEnumerable<BoxType> BuildTypes(string? availableBoxTypes)
        //{
        //    if (string.IsNullOrWhiteSpace(availableBoxTypes))
        //        return Enumerable.Empty<BoxType>();

        //    var boxTypes = availableBoxTypes
        //        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        //        .Select(x => x.Trim())
        //        .ToHashSet(StringComparer.OrdinalIgnoreCase);

        //    return new[]
        //    {
        //        ("CASE", BoxType.CaseLabel),
        //        ("UB", BoxType.UnitBox),
        //        ("AUB", BoxType.AdditionalUnitBox),
        //        ("OUB", BoxType.OuterUnitBox),
        //        ("CB", BoxType.CartonBox),
        //        ("OCB", BoxType.OuterCartonBox),
        //        ("ACB", BoxType.AdditionalCartonBox)
        //    }
        //    .Where(x => boxTypes.Contains(x.Item1))
        //    .Select(x => x.Item2);
        //}
    }
}
