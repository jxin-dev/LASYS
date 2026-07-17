namespace LASYS.UIControls.Controls
{
    public class ModalOverlay
    {
        private readonly Control _parent;
        private OverlayForm? _overlay;
        private int _dialogCount;
        public ModalOverlay(Control parent)
        {
            _parent = parent;
        }

        private OverlayForm CreateOverlay()
        {
            _parent.Update();

            return new OverlayForm
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                BackColor = Color.Black,
                Opacity = 0.5,
                Bounds = new Rectangle(
                    _parent.PointToScreen(Point.Empty),
                    _parent.ClientSize)
            };
        }
        public void HideOverlay()
        {
            _dialogCount = 0;

            _overlay?.Close();
            _overlay?.Dispose();
            _overlay = null;
        }

        public void Show(Form dialog)
        {
            if (_overlay == null)
            {
                _overlay = CreateOverlay();
                _overlay.Show(_parent.FindForm());
            }

            _dialogCount++;

            var owner = dialog.Owner ?? Form.ActiveForm ?? _parent.FindForm();

            dialog.Owner = owner;
            dialog.StartPosition = FormStartPosition.Manual;

            dialog.Location = new Point(
                _overlay.Left + (_overlay.Width - dialog.Width) / 2,
                _overlay.Top + (_overlay.Height - dialog.Height) / 2);

            dialog.FormClosed += Dialog_FormClosed;

            owner.Activated += Owner_Activated;

            dialog.FormClosed += (_, _) =>
            {
                owner.Activated -= Owner_Activated;
            };

            dialog.Show(owner);

            void Owner_Activated(object? sender, EventArgs e)
            {
                if (!dialog.IsDisposed && dialog.Visible)
                {
                    dialog.BringToFront();
                    dialog.Activate();
                }
            }
        }

        private void Dialog_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is Form form)
                form.FormClosed -= Dialog_FormClosed;

            _dialogCount = Math.Max(0, _dialogCount - 1);

            if (_dialogCount == 0)
            {
                _overlay?.Close();
                _overlay?.Dispose();
                _overlay = null;
            }
        }
    }
}
