using LASYS.UIControls.Models;
using System.Drawing;
using System.IO;
namespace LASYS.UIControls.Controls
{
    public class SideNavigation : Panel
    {
        private readonly List<(NavItem Item, NavButton Button, List<NavButton> SubButtons)> _buttons = new();
       
        private readonly Panel _profilePanel;
        private readonly ProfileAvatar _avatar;
        private readonly Label _username;
        private readonly Label _sectionName;

        public Color ProfilePanelColor { get; set; } = Color.FromArgb(10, 95, 80);
        public Color BackgroundColor { get; set; } = Color.FromArgb(15, 127, 102);
        public Color SubItemColor { get; set; } = Color.FromArgb(20, 90, 80); //Color.FromArgb(31, 42, 46);
        public Color HoverColor { get; set; } = Color.FromArgb(0, 166, 147);
        public Color ActiveColor { get; set; } = Color.FromArgb(0, 140, 125); // darker green

        public SideNavigation()
        {
            AutoScroll = true;
            Dock = DockStyle.Left;
            Width = 220;
            BackColor = BackgroundColor;
            // === Profile Section ===
            _profilePanel = new Panel
            {
                Height = 140,
                Dock = DockStyle.Top,
                BackColor = ProfilePanelColor
            };

            _profilePanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(0, 166, 147), 2);
                e.Graphics.DrawLine(pen, 0, _profilePanel.Height - 1, _profilePanel.Width, _profilePanel.Height - 1);
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Avatar
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Username
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); // Section

            _avatar = new ProfileAvatar
            {
                Size = new Size(70, 70),
                //Location = new Point((220 - 70) / 2, 20),
                Anchor = AnchorStyles.None,
                Cursor = Cursors.Hand,
                ProfileImage = CreateDefaultAvatar()
            };

            _username = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Guest"
            };

            _sectionName = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Section Name"
            };

            // Add controls to table
            table.Controls.Add(_avatar, 0, 0);
            table.Controls.Add(_username, 0, 1);
            table.Controls.Add(_sectionName, 0, 2);

            _profilePanel.Controls.Add(table);
            Controls.Add(_profilePanel);
        }

        private Image CreateDefaultAvatar(string username = "User")
        {
            int size = 70;
            string initials = username.Length > 0 ? username.Substring(0, 1).ToUpper() : "U";

            // Generate a unique soft background color based on username
            int hash = username.GetHashCode();
            Color baseColor = Color.FromArgb(255,
                100 + (hash & 0x7F),         // R
                100 + ((hash >> 8) & 0x7F),  // G
                100 + ((hash >> 16) & 0x7F)  // B
            );

            Bitmap bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Circular background
                using (SolidBrush brush = new SolidBrush(baseColor))
                    g.FillEllipse(brush, 0, 0, size - 1, size - 1);

                // Draw initial (no border)
                using (Font font = new Font("Segoe UI", 22, FontStyle.Bold, GraphicsUnit.Pixel))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(initials, font, Brushes.White, new RectangleF(0, 0, size, size), sf);
                }
            }

            return bmp;
        }

        public void SetProfile(string username, string sectionName, string? imagePath)
        {
            _username.Text = username;
            _sectionName.Text = sectionName;

            Image? avatar = null;

            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Load and clone image into memory safely
                        using (var temp = Image.FromStream(fs, false, false))
                        {
                            avatar = new Bitmap(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log or show a message for debugging
                    Console.WriteLine($"[Profile] Failed to load image: {ex.Message}");
                }
            }

            // Always fallback to default if anything fails
            _avatar.ProfileImage = avatar ?? CreateDefaultAvatar(username);
            _avatar.Invalidate();
        }
        public void AddItem(NavItem item)
        {
            var mainButton = CreateButton(item);
            Controls.Add(mainButton);
            _buttons.Add((item, mainButton, new List<NavButton>()));

            if (item.SubItems != null && item.SubItems.Any())
            {
                foreach (var sub in item.SubItems)
                {
                    var subButton = CreateButton(sub, isSub: true);
                    subButton.Visible = false;
                    _buttons.Last().SubButtons.Add(subButton);
                    Controls.Add(subButton);
                }

                mainButton.Click += (_, _) => ToggleSubItems(item);
            }

            RearrangeButtons();
        }

        private NavButton CreateButton(NavItem item, bool isSub = false)
        {
            var baseColor = isSub ? SubItemColor : BackgroundColor;
            var hoverColor = HoverColor;
            var activeColor = ActiveColor;
            var btn = new NavButton
            {
                //Text = (isSub ? "   • " : "") + item.Text,
                Text = item.Text,
                IsSubButton = isSub,
                Dock = DockStyle.None,
                Height = 40,
                Width = this.Width,
                BackColor = isSub ? SubItemColor : BackgroundColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                Padding = isSub  ? new Padding(40, 0, 0, 0) : new Padding(20, 0, 0, 0)
            };

            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.BorderSize = 0;

            btn.FlatAppearance.MouseOverBackColor = hoverColor;
            btn.FlatAppearance.MouseDownBackColor = hoverColor;

            btn.MouseEnter += (_, _) =>
            {
                //btn.BackColor = hoverColor;
                if (btn.BackColor != activeColor)
                    btn.BackColor = hoverColor;
            };

            btn.MouseLeave += (_, _) =>
            {
                //btn.BackColor = baseColor;
                if (btn.BackColor != activeColor)
                    btn.BackColor = baseColor;
            };
            // Click event
            btn.Click += (_, _) =>
            {
                item.RaiseClicked();
                foreach (var tuple in _buttons)
                {
                    // MAIN BUTTON
                    bool isActiveMain = tuple.Button == btn;

                    tuple.Button.IsActive = isActiveMain;
                    tuple.Button.BackColor =
                        isActiveMain
                        ? ActiveColor
                        : (tuple.Button.IsSubButton ? SubItemColor : BackgroundColor);

                    tuple.Button.Invalidate(); // 🔥 redraw indicator

                    // SUB BUTTONS
                    foreach (var sb in tuple.SubButtons)
                    {
                        bool isActiveSub = sb == btn;

                        sb.IsActive = isActiveSub;
                        sb.BackColor = isActiveSub ? ActiveColor : SubItemColor;
                        sb.Invalidate();
                    }
                    
                }
            };

            return btn;   
        }

        public void SelectItem(NavItem? targetItem)
        {
            if (targetItem == null)
                return;

            foreach (var (item, button, subs) in _buttons)
            {
                if (item == targetItem)
                {
                    button.PerformClick();
                    return;
                }

                foreach (var sub in subs)
                {
                    if (sub.Tag == targetItem)
                    {
                        sub.PerformClick();
                        return;
                    }
                }
            }
        }

        private void ToggleSubItems(NavItem parent)
        {
            var tuple = _buttons.FirstOrDefault(b => b.Item == parent);
            if (tuple.Item == null) return;

            bool anyVisible = tuple.SubButtons.Any(s => s.Visible);
            foreach (var s in tuple.SubButtons)
                s.Visible = !anyVisible;

            RearrangeButtons();
        }

        private void RearrangeButtons()
        {
            int y = _profilePanel.Bottom + 5;
            foreach (var (item, button, subs) in _buttons)
            {
                button.Location = new Point(0, y);
                y += button.Height;

                foreach (var sub in subs)
                {
                    if (sub.Visible)
                    {
                        sub.Location = new Point(0, y);
                        y += sub.Height;
                    }
                }
            }
        }

    }
}
