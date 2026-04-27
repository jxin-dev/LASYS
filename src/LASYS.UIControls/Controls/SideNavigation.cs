using LASYS.UIControls.Models;
using System.Drawing;
using System.IO;
namespace LASYS.UIControls.Controls
{
    public class SideNavigation : Panel
    {
        private readonly List<(
             NavItem Item,
             NavButton Button,
             List<(NavItem SubItem, NavButton SubButton)> SubButtons
         )> _buttons = new();

        private readonly Panel _profilePanel;
        private readonly ProfileAvatar _avatar;
        private readonly Label _username;
        private readonly Label _sectionName;
        private readonly Panel _menuPanel;

        public Color ProfilePanelColor { get; set; } = Color.FromArgb(10, 95, 80);
        public Color BackgroundColor { get; set; } = Color.FromArgb(15, 127, 102);
        public Color SubItemColor { get; set; } = Color.FromArgb(20, 90, 80);
        public Color HoverColor { get; set; } = Color.FromArgb(0, 166, 147);
        public Color ActiveColor { get; set; } = Color.FromArgb(0, 140, 125);

        public SideNavigation()
        {
            AutoScroll = false;
            Dock = DockStyle.Left;
            Width = 220;
            BackColor = BackgroundColor;

            _profilePanel = new Panel
            {
                Height = 140,
                Dock = DockStyle.Top,
                BackColor = ProfilePanelColor
            };

            _menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true 
            };

            _avatar = new ProfileAvatar
            {
                Size = new Size(70, 70),
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
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Section Name"
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            table.Controls.Add(_avatar, 0, 0);
            table.Controls.Add(_username, 0, 1);
            table.Controls.Add(_sectionName, 0, 2);

            _profilePanel.Controls.Add(table);

            Controls.Add(_menuPanel);
            Controls.Add(_profilePanel);
        }

        private Image CreateDefaultAvatar(string username = "User")
        {
            int size = 70;
            string initials = username.Length > 0 ? username.Substring(0, 1).ToUpper() : "U";

            int hash = username.GetHashCode();
            Color baseColor = Color.FromArgb(255,
                100 + (hash & 0x7F),
                100 + ((hash >> 8) & 0x7F),
                100 + ((hash >> 16) & 0x7F));

            Bitmap bmp = new(size, size);
            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using var brush = new SolidBrush(baseColor);
            g.FillEllipse(brush, 0, 0, size - 1, size - 1);

            using var font = new Font("Segoe UI", 22, FontStyle.Bold, GraphicsUnit.Pixel);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.DrawString(initials, font, Brushes.White, new RectangleF(0, 0, size, size), sf);

            return bmp;
        }

        public void SetProfile(string username, string sectionName, string? imagePath)
        {
            _username.Text = username;
            _sectionName.Text = sectionName;
        }

        public void AddItem(NavItem item)
        {
            var mainButton = CreateButton(item);
            //Controls.Add(mainButton);
            _menuPanel.Controls.Add(mainButton);
            _menuPanel.Controls.SetChildIndex(mainButton, 0);


            var subList = new List<(NavItem, NavButton)>();
            _buttons.Add((item, mainButton, subList));

            if (item.SubItems != null && item.SubItems.Any())
            {
                foreach (var sub in item.SubItems)
                {
                    var subButton = CreateButton(sub, isSub: true);
                    subButton.Visible = false;

                    subList.Add((sub, subButton));
                    //Controls.Add(subButton);
                    _menuPanel.Controls.Add(subButton);
                    _menuPanel.Controls.SetChildIndex(subButton, 0);

                }

                mainButton.Click += (_, _) => ToggleSubItems(item);
            }

            RearrangeButtons();
            //Controls.SetChildIndex(_profilePanel, 0);
        }

        private NavButton CreateButton(NavItem item, bool isSub = false)
        {
            var btn = new NavButton
            {
                Text = item.Text,
                IsSubButton = isSub,
                Height = 40,
                //Width = Width,
                Dock = DockStyle.Top,
                BackColor = isSub ? SubItemColor : BackgroundColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Padding = isSub ? new Padding(40, 0, 0, 0) : new Padding(20, 0, 0, 0)
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (_, _) =>
            {
                if (!btn.IsActive)
                    btn.BackColor = HoverColor;
            };

            btn.MouseLeave += (_, _) =>
            {
                if (!btn.IsActive)
                    btn.BackColor = btn.IsSubButton ? SubItemColor : BackgroundColor;
            };

            // 🔥 IMPORTANT: no active UI logic here anymore
            btn.Click += (_, _) => item.RaiseClicked();

            return btn;
        }

        public void SetActiveItem(NavItem? targetItem)
        {
            if (targetItem == null) return;

            foreach (var (item, button, subs) in _buttons)
            {
                bool isMainActive = item == targetItem;

                button.IsActive = isMainActive;
                button.BackColor = isMainActive ? ActiveColor : BackgroundColor;
                button.Invalidate();

                bool hasActiveSub = subs.Any(s => s.SubItem == targetItem);

                // Auto expand if sub is active
                foreach (var (_, subButton) in subs)
                    subButton.Visible = hasActiveSub;

                foreach (var (subItem, subButton) in subs)
                {
                    bool isSubActive = subItem == targetItem;

                    subButton.IsActive = isSubActive;
                    subButton.BackColor = isSubActive ? ActiveColor : SubItemColor;
                    subButton.Invalidate();
                }
            }
        }

        private void ToggleSubItems(NavItem parent)
        {
            var tuple = _buttons.FirstOrDefault(b => b.Item == parent);
            if (tuple.Item == null) return;

            bool anyVisible = tuple.SubButtons.Any(s => s.SubButton.Visible);

            foreach (var (_, subButton) in tuple.SubButtons)
                subButton.Visible = !anyVisible;

            RearrangeButtons();
        }

        private void RearrangeButtons()
        {
            int y = _profilePanel.Bottom + 5;

            foreach (var (_, button, subs) in _buttons)
            {
                button.Location = new Point(0, y);
                y += button.Height;

                foreach (var (_, sub) in subs)
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
