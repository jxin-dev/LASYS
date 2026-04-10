
namespace LASYS.UIControls.Controls
{
    public class NavButton : Button
    {
        public bool IsActive { get; set; }
        public bool IsExpanded { get; set; }
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (IsActive)
            {
                using var brush = new SolidBrush(Color.FromArgb(0, 200, 180));
                e.Graphics.FillRectangle(brush, 0, 0, 4, Height);
            }
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
