using System.ComponentModel;

namespace LASYS.UIControls.Controls
{
    public class LoadingLabel : Label
    {
        private readonly System.Windows.Forms.Timer _timer;
        private int _dotCount = 0;
        private string _baseText = "Please wait";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnimationSpeed { get; set; } = 500; // in milliseconds
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxDots { get; set; } = 3;           // number of dots to cycle through

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BaseText
        {
            get => _baseText;
            set
            {
                _baseText = value;
                Text = _baseText;
            }
        }

        public LoadingLabel()
        {
            AutoSize = true;
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = AnimationSpeed;
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _dotCount = (_dotCount + 1) % (MaxDots + 1);
            Text = _baseText + new string('.', _dotCount);
        }

        public void Start()
        {
            _dotCount = 0;
            Text = _baseText;
            _timer.Start();
            Visible = true;
        }

        public void Stop()
        {
            _timer.Stop();
            _dotCount = 0;
            Text = _baseText; // Reset text
            Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _timer.Dispose();

            base.Dispose(disposing);
        }
    }
}
