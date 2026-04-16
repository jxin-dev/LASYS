using LASYS.DesktopApp.Views.Forms;

namespace LASYS.DesktopApp.Views.Interfaces
{
    public interface ILabelBoxTypeView
    {
        void RenderButtons(IEnumerable<LabelBoxType> types);
        LabelBoxType? SelectedType { get; }
        DialogResult ShowDialog();
    }
}
