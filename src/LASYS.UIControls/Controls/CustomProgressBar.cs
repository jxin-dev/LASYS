using System.ComponentModel;

namespace LASYS.UIControls.Controls
{
    [DefaultEvent("Click")]
    public class CustomProgressBar : Panel
    {
        private readonly Panel _fillPanel;
        private readonly Label _statusLabel;
        private int _value = 0;
        private int _maximum = 100;
        private Color _progressColor = Color.FromArgb(0, 122, 204);
        private Color _backgroundColor = Color.LightGray;
        private bool _showPercentage = false;
        private readonly object _animationLock = new();

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Gets or sets the current progress value.")]
        public int Value
        {
            get => _value;
            set
            {
                int clamped = Math.Max(0, Math.Min(value, Maximum));
                if (_value != clamped)
                {
                    _value = clamped;
                    _ = AnimateProgressAsync();
                }
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Gets or sets the maximum progress value.")]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(1, value);
                _ = AnimateProgressAsync();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Gets or sets the color of the progress fill.")]
        public Color ProgressColor
        {
            get => _progressColor;
            set
            {
                _progressColor = value;
                _fillPanel.BackColor = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Gets or sets the background color of the progress bar.")]
        public Color ProgressBackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Show or hide percentage beside the status message.")]
        public bool ShowPercentage
        {
            get => _showPercentage;
            set
            {
                _showPercentage = value;
                UpdateStatusText();
            }
        }

        public CustomProgressBar()
        {
            DoubleBuffered = true;
            Height = 45; // Includes label + bar
            BackColor = Color.Transparent;
            BorderStyle = BorderStyle.None;

            // Status label (above the progress bar)
            _statusLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                Padding = new Padding(2, 0, 0, 0),
                Text = "Initializing..."
            };

            // Progress bar background (container for fill)
            var barBackground = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _backgroundColor,
                Margin = new Padding(0, 5, 0, 0),
                Padding = new Padding(0),
                Height = 25
            };

            // Progress fill
            _fillPanel = new Panel
            {
                BackColor = _progressColor,
                Width = 0,
                Dock = DockStyle.Left
            };

            barBackground.Controls.Add(_fillPanel);

            Controls.Add(barBackground);
            Controls.Add(_statusLabel);

            Resize += (_, _) => UpdateWidthInstant();
        }

        private void UpdateWidthInstant()
        {
            double ratio = (double)_value / _maximum;
            int targetWidth = (int)(Width * ratio);
            _fillPanel.Width = targetWidth;
        }

        private async Task AnimateProgressAsync()
        {
            if (Maximum <= 0 || Width <= 0)
                return;

            double ratio = (double)_value / _maximum;
            int targetWidth = (int)(Width * ratio);
            int currentWidth = _fillPanel.Width;
            int step = Math.Max(1, Math.Abs(targetWidth - currentWidth) / 15);

            if (targetWidth > currentWidth)
            {
                while (_fillPanel.Width < targetWidth)
                {
                    _fillPanel.Width = Math.Min(targetWidth, _fillPanel.Width + step);
                    await Task.Delay(10);
                }
            }
            else if (targetWidth < currentWidth)
            {
                while (_fillPanel.Width > targetWidth)
                {
                    _fillPanel.Width = Math.Max(targetWidth, _fillPanel.Width - step);
                    await Task.Delay(10);
                }
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (_showPercentage)
                _statusLabel.Text = $"{_value}% - {_statusLabel.Text}";
        }

        /// <summary>
        /// Updates only the status text displayed above the progress bar.
        /// </summary>
        public void UpdateStatus(string message)
        {
            if (_showPercentage)
                _statusLabel.Text = $"{_value}% - {message}";
            else
                _statusLabel.Text = message;
        }

        /// <summary>
        /// Updates both progress and status text.
        /// </summary>
        public void UpdateProgress(int value, string message)
        {
            Value = value;
            UpdateStatus(message);
        }

        public void Increment(int amount)
        {
            Value = Math.Min(Maximum, Value + amount);
        }

        public void Reset()
        {
            _value = 0;
            _fillPanel.Width = 0;
            _statusLabel.Text = "Initializing...";
        }
    }
}
