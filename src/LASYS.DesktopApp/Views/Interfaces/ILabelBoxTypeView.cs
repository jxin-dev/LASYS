using LASYS.Application.Common.Enums;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILabelBoxTypeView
    {
        void RenderButtons(IReadOnlyCollection<BoxType> boxTypes);
        BoxType? SelectedType { get; }
        DialogResult ShowDialog();
    }
}
