namespace LASYS.UIControls.Controls
{
    public class DraggableResizerPanel : Panel
    {
        private Color AccentGreen       = Color.FromArgb(0, 166, 147);
        private Color LightBackground   = Color.FromArgb(245, 248, 247);
        private Color SoftGray          = Color.FromArgb(220, 230, 228);
        private Color TextDark          = Color.FromArgb(60, 60, 60);


        private System.Windows.Forms.Timer? _animationTimer;
        private Panel resizerBar;
        private Panel toolbar;
        private Panel contentPanel;
        private Panel navigatorPanel;

        private PictureBox btnClose;

        private bool isDragging = false;
        private int startY;
        private int startHeight;

        public int MinimumPanelHeight { get; set; } = 200;
        public int DefaultPanelHeight { get; set; } = 300;
        public Panel Content => contentPanel;
        public double HeightPercentage { get; set; } = 0.7;

        private readonly Dictionary<string, Control> tabContents = new();
        private Control? activeContent = null;

        public DraggableResizerPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            UpdateStyles();


            this.Dock = DockStyle.Bottom;
            this.BackColor = LightBackground;

            this.Paint += (s, e) =>
            {
                using var pen = new Pen(SoftGray, 1);
                e.Graphics.DrawLine(pen, 0, 0, Width, 0);
            };

            // ----- Toolbar -----
            toolbar = new Panel
            {
                Height = 20,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            toolbar.Paint += (s, e) =>
            {
                using var pen = new Pen(SoftGray);
                e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            // ----- Navigator Panel -----
            navigatorPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(230, 240, 238)
            };

            // ----- Resizer Bar -----
            resizerBar = new Panel
            {
                Height = 5,
                Dock = DockStyle.Top,
                Cursor = Cursors.SizeNS,
                BackColor = AccentGreen
            };
            resizerBar.MouseDown += Resizer_MouseDown;
            resizerBar.MouseMove += Resizer_MouseMove;
            resizerBar.MouseUp += Resizer_MouseUp;


            btnClose = new PictureBox
            {
                Size = new Size(toolbar.Height - 5, toolbar.Height - 5),
                Image = Image.FromFile(@"Resources\close_gray.png"),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnClose.Click += (s, e) => HidePanel();
            toolbar.Controls.Add(btnClose);

            // Position buttons on toolbar resize
            toolbar.Resize += (s, e) =>
            {
                btnClose.Location = new Point((toolbar.Width - btnClose.Width) - 1, 1);
            };

            // ----- Content Panel -----
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(toolbar);
            this.Controls.Add(navigatorPanel);
            this.Controls.Add(resizerBar);


            contentPanel.BringToFront();

            HidePanel();

        }


        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // WS_EX_COMPOSITED
                return cp;
            }
        }
        public void AddTab(string title, Control contentControl)
        {
            if (tabContents.ContainsKey(title))
                return; // prevent duplicate tab

            int spacing = 4;
            int x = navigatorPanel.Controls.Count > 0
                ? navigatorPanel.Controls[navigatorPanel.Controls.Count - 1].Right + spacing
                : spacing;

            var lblTab = new Label
            {
                Text = title,
                AutoSize = true,
                Padding = new Padding(12, 6, 12, 6),
                Margin = Padding.Empty,
                BackColor = Color.Silver,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            lblTab.Location = new Point(x, (navigatorPanel.Height - lblTab.PreferredHeight) / 2);

            lblTab.BackColor = Color.White;
            lblTab.ForeColor = TextDark;


            lblTab.Click += (sender, e) =>
            {
                if (activeContent == contentControl)
                {
                    TogglePanel();
                    return; // 🔥 STOP HERE (VERY IMPORTANT)
                }

                // SWITCH TAB
                ShowPanel();

                if (activeContent != null)
                    activeContent.Visible = false;

                if (!contentPanel.Controls.Contains(contentControl))
                    contentPanel.Controls.Add(contentControl);

                activeContent = contentControl;
                activeContent.Visible = true;

                UpdateTabStyles(title);
            };


            lblTab.MouseEnter += (sender, e) =>
            {
                Cursor = Cursors.Hand;
                if (activeContent != contentControl)
                    lblTab.BackColor = Color.FromArgb(200, 230, 225);
            };

            lblTab.MouseLeave += (sender, e) =>
            {
                Cursor = Cursors.Default;
                if (activeContent != contentControl)
                {
                    lblTab.BackColor = Color.White;
                    lblTab.ForeColor = TextDark;
                }
            };

            lblTab.Paint += (s, e) =>
            {
                if (activeContent == contentControl)
                {
                    using var pen = new Pen(AccentGreen, 2);
                    e.Graphics.DrawLine(pen, 0, lblTab.Height - 1, lblTab.Width, lblTab.Height - 1);
                }
            };

            navigatorPanel.Controls.Add(lblTab);
            tabContents[title] = contentControl;
        }
        private void UpdateTabStyles(string title)
        {
            foreach (Label tab in navigatorPanel.Controls.OfType<Label>())
            {
                if (tab.Text == title)
                {
                    tab.BackColor = AccentGreen;
                    tab.ForeColor = Color.White;
                    tab.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else
                {
                    tab.BackColor = Color.White;
                    tab.ForeColor = TextDark;
                    tab.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }
            }
        }
        private void TogglePanel()
        {
            if (contentPanel.Visible == true && toolbar.Visible == true && resizerBar.Visible == true)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        private void ShowPanel()
        {
            if (this.Parent == null)
                return;

            // previous animation
            _animationTimer?.Stop();
            _animationTimer?.Dispose();

            int maxHeight = (int)(this.Parent.ClientSize.Height * HeightPercentage);
            int targetHeight = Math.Max(MinimumPanelHeight, Math.Min(DefaultPanelHeight, maxHeight));

            // Show sections first
            contentPanel.Visible = true;
            toolbar.Visible = true;
            resizerBar.Visible = true;

            this.Show();
            this.BringToFront();

            // Start from collapsed height
            int startHeight = this.Height;
            int step = 20;

            var timer = new System.Windows.Forms.Timer { Interval = 10 };

            timer.Tick += (s, e) =>
            {
                if (this.Height < targetHeight)
                {
                    this.Height = Math.Min(targetHeight, this.Height + step);
                }
                else
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }

     
        public void HidePanel()
        {
            _animationTimer?.Stop();
            _animationTimer?.Dispose();

            contentPanel.Visible = false;
            toolbar.Visible = false;
            resizerBar.Visible = false;

            this.Height = navigatorPanel.Height;
        }

        private void Resizer_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Resizer_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                startY = Cursor.Position.Y;
                startHeight = this.Height;

            }
        }

        private void Resizer_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && this.Parent != null)
            {
                this.SuspendLayout();

                int delta = Cursor.Position.Y - startY;
                int newHeight = startHeight - delta;

                int maxHeight = (int)(this.Parent.ClientSize.Height * HeightPercentage);
                newHeight = Math.Max(MinimumPanelHeight, Math.Min(newHeight, maxHeight));

                this.Height = newHeight;

                this.ResumeLayout(true);

            }
        }

        public void ShowTab(string title, bool allowToggle = true)
        {
            if (!tabContents.TryGetValue(title, out var contentControl))
                return; // tab not found

            if (activeContent == contentControl)
            {
                if (allowToggle)
                    TogglePanel();   // Only toggle if allowed
            }
            else
            {
                ShowPanel(); // Ensure panel is visible
            }

            // Hide currently active content
            if (activeContent != null)
                activeContent.Visible = false;

            // Add new content if not already added
            if (!contentPanel.Controls.Contains(contentControl))
                contentPanel.Controls.Add(contentControl);

            activeContent = contentControl;
            activeContent.Visible = true;

            // Optionally, update tab appearance if needed (highlight active tab)
            foreach (Label tab in navigatorPanel.Controls.OfType<Label>())
            {
                if (tab.Text == title)
                {
                    tab.BackColor = AccentGreen;
                    tab.ForeColor = Color.White;
                    tab.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else
                {
                    tab.BackColor = Color.White;
                    tab.ForeColor = TextDark;
                    tab.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                }

            }
        }

    }
}
