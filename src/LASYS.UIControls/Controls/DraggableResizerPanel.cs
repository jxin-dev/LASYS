namespace LASYS.UIControls.Controls
{
    public class DraggableResizerPanel : Panel
    {
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
            this.BackColor = Color.LightSlateGray;

            // ----- Toolbar -----
            toolbar = new Panel
            {
                Height = 20,
                Dock = DockStyle.Top,
                BackColor = Color.MintCream
            };

            // ----- Navigator Panel -----
            navigatorPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Bottom,
                BackColor = Color.SeaGreen
            };

            // ----- Resizer Bar -----
            resizerBar = new Panel
            {
                Height = 5,
                Dock = DockStyle.Top,
                Cursor = Cursors.SizeNS,
                BackColor = Color.SeaGreen
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
                BackColor = Color.WhiteSmoke
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
                Padding = new Padding(10, 4, 10, 4),
                Margin = Padding.Empty,
                BackColor = Color.Silver,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            lblTab.Location = new Point(x, (navigatorPanel.Height - lblTab.PreferredHeight) / 2);

            //foreach (Label tab in navigatorPanel.Controls.OfType<Label>()) { }
            //    tab.BackColor = Color.Silver;

            lblTab.BackColor = Color.MediumSeaGreen;
            lblTab.ForeColor = Color.WhiteSmoke;


            lblTab.Click += (sender, e) =>
            {
                if (activeContent == contentControl)
                {
                    TogglePanel();
                }
                else
                {
                    ShowPanel();
                }

                // Show the existing control (if not already shown)
                if (activeContent != null)
                    activeContent.Visible = false;

                if (!contentPanel.Controls.Contains(contentControl))
                    contentPanel.Controls.Add(contentControl);

                activeContent = contentControl;
                activeContent.Visible = true;
            };

            lblTab.MouseEnter += (sender, e) =>
            {
                lblTab.BackColor = Color.Red;
                lblTab.ForeColor = Color.White;
            };

            lblTab.MouseLeave += (sender, e) =>
            {
                lblTab.BackColor = Color.MediumSeaGreen;
                lblTab.ForeColor = Color.WhiteSmoke;


            };


            navigatorPanel.Controls.Add(lblTab);
            tabContents[title] = contentControl;
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
            if (this.Parent != null)
            {
                int maxHeight = (int)(this.Parent.ClientSize.Height * HeightPercentage);
                this.Height = Math.Max(DefaultPanelHeight, Math.Min(this.Height, maxHeight));
            }


            // Show all sections
            contentPanel.Visible = true;
            toolbar.Visible = true;
            resizerBar.Visible = true;

            this.Show();
            this.BringToFront(); // optional: ensure it's top-most in z-order
        }

        public void HidePanel()
        {
            // Collapse everything except navigatorPanel
            contentPanel.Visible = false;
            toolbar.Visible = false;
            resizerBar.Visible = false;

            // Collapse the panel height to navigator only
            this.Height = navigatorPanel.Height;

            //this.Hide();
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

        public void ShowTab(string title)
        {
            if (!tabContents.TryGetValue(title, out var contentControl))
                return; // tab not found

            if (activeContent == contentControl)
            {
                TogglePanel(); // Hide if already active
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
                tab.BackColor = Color.MediumSeaGreen;
                tab.ForeColor = Color.WhiteSmoke;
                //if (tab.Text == title)
                //{
                //    tab.BackColor = Color.MediumSeaGreen;
                //    tab.ForeColor = Color.WhiteSmoke;
                //}
                //else
                //{
                //    tab.BackColor = Color.Silver;
                //    tab.ForeColor = Color.Black;
                //}
            }
        }

    }
}
