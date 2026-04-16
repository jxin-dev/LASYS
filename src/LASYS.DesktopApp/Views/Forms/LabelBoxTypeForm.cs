using LASYS.DesktopApp.Views.Interfaces;

namespace LASYS.DesktopApp.Views.Forms
{
    public partial class LabelBoxTypeForm : Form, ILabelBoxTypeView
    {
        public LabelBoxType? SelectedType { get; private set; }
        public LabelBoxTypeForm()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MinimumSize = new Size(250, 0);

            flowButtons.FlowDirection = FlowDirection.TopDown;
            flowButtons.WrapContents = false;
            flowButtons.AutoSize = true;
            flowButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowButtons.Padding = new Padding(5);
            flowButtons.BackColor = Color.White;
            flowButtons.Dock = DockStyle.Top;

            flowButtons.SizeChanged += (s, e) =>
            {
                foreach (Button btn in flowButtons.Controls.OfType<Button>())
                {
                    btn.Width = flowButtons.ClientSize.Width - 10;
                }
            };
        }
        private readonly Dictionary<LabelBoxType, string> _buttonLabels = new()
        {
            { LabelBoxType.CaseLabel, "Case Label" },
            { LabelBoxType.UnitBox, "Unit Box" },
            { LabelBoxType.AdditionalUnitBox, "Additional Unit Box" },
            { LabelBoxType.OuterUnitBox, "Outer Unit Box" },
            { LabelBoxType.CartonBox, "Carton Box" },
            { LabelBoxType.OuterCartonBox, "Outer Carton Box" },
            { LabelBoxType.AdditionalCartonBox, "Additional Carton Box" }
        };
        private void ResizeForm()
        {
            this.Height =
                lblHeader.Height +
                flowButtons.Height;
        }
        private Button CreateButton(string text, bool isCancel = false)
        {
            var btn = new Button
            {
                Text = text,
                Height = 45,
                Width = 250,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 5),
                Font = new Font("Segoe UI Semibold", 9),
                BackColor = Color.FromArgb(240, 240, 240), // light background
                ForeColor = Color.FromArgb(0, 150, 136),   // teal text
                FlatAppearance =
                {
                    BorderSize = 0,
                    MouseOverBackColor = Color.FromArgb(220, 240, 238),
                    MouseDownBackColor = Color.FromArgb(200, 230, 228)
                },
                Cursor = Cursors.Hand
            };


            if (isCancel)
            {
                btn.BackColor = Color.FromArgb(15, 127, 102);
                btn.ForeColor = Color.White;

                btn.FlatAppearance.BorderSize = 0;

                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 150, 120);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(10, 100, 80);

                btn.Click += (s, e) =>
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                };
            }

            return btn;
        }
        public void RenderButtons(IEnumerable<LabelBoxType> types)
        {
            flowButtons.SuspendLayout();
            flowButtons.Controls.Clear();

            foreach (var type in types)
            {
                var text = _buttonLabels.TryGetValue(type, out var label)
                    ? label
                    : type.ToString();

                var btn = CreateButton(text);
                btn.Tag = type;

                btn.Click += OnButtonClick;

                flowButtons.Controls.Add(btn);
            }

            // Always last
            flowButtons.Controls.Add(CreateButton("Cancel", true));

            flowButtons.ResumeLayout();

            ResizeForm();
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is LabelBoxType type)
            {
                SelectedType = type;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    public enum LabelBoxType
    {
        CaseLabel,
        UnitBox,
        AdditionalUnitBox,
        OuterUnitBox,
        CartonBox,
        OuterCartonBox,
        AdditionalCartonBox,
    }
}
