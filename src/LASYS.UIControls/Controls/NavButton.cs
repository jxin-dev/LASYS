using System.ComponentModel;


namespace LASYS.UIControls.Controls
{
    public class NavButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSubButton { get; set; }

        public NavButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            TextAlign = ContentAlignment.MiddleLeft;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            Padding = new Padding(20, 0, 0, 0);
            Height = 40;
            Dock = DockStyle.Top;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }
    }
}
