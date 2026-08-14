using LASYS.Cleanup.UI.Enums;

namespace LASYS.Cleanup.UI.Views.Forms
{
    public partial class ConfigurationForm : Form
    {
        private readonly DayOfWeek _weeklyExecutionDay = DayOfWeek.Monday;
        private readonly int _monthlyExecutionDay = 1;

        public ConfigurationForm()
        {
            InitializeComponent();
            cmbRetentionUnit.DataSource = Enum.GetValues<RetentionUnit>();
            cmbFrequency.DataSource = Enum.GetValues<ScheduleFrequency>();
            InitializeTimeComboBox();
        }

        private void UpdateScheduleStatus()
        {
            if (cmbFrequency.SelectedItem is not ScheduleFrequency frequency)
                return;

            if (cmbRunTime.SelectedItem is not string time)
                return;

            string statusText = frequency switch
            {
                ScheduleFrequency.Daily => $"Cleanup will run every day at {time}.",

                ScheduleFrequency.Weekly => $"Cleanup will run every {_weeklyExecutionDay} at {time}.",

                ScheduleFrequency.Monthly =>
                    $"Cleanup will run on the " +
                    $"{GetOrdinal(_monthlyExecutionDay)} " +
                    $"of every month at {time}.",

                _ =>
                    $"Cleanup will run at {time}."
            };

            lblScheduleStatus.Text = $"      {statusText}";
        }
        private static string GetOrdinal(int number)
        {
            if (number % 100 is >= 11 and <= 13)
                return $"{number}th";

            return (number % 10) switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };
        }

        private void UpdateRetentionStatus()
        {
            if (cmbRetentionUnit.SelectedItem is not RetentionUnit unit)
                return;

            int value = (int)nudRetention.Value;

            string statusText = unit switch
            {
                RetentionUnit.Hours =>
                    $"Files older than {value} hour{(value == 1 ? "" : "s")} will be deleted during cleanup.",

                RetentionUnit.Days =>
                    $"Files older than {value} day{(value == 1 ? "" : "s")} will be deleted during cleanup.",

                RetentionUnit.Months =>
                    $"Files older than {value} month{(value == 1 ? "" : "s")} will be deleted during cleanup.",

                _ =>
                    "Files older than the specified retention period will be deleted during cleanup."
            };

            lblRetentionStatus.Text =
                $"      {statusText}";
        }
        private void cmbRetentionUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRetentionUnit.SelectedItem is not RetentionUnit unit)
                return;
            nudRetention.Value = 1;
            switch (unit)
            {
                case RetentionUnit.Hours:
                    nudRetention.Minimum = 1;
                    nudRetention.Maximum = 24;
                    break;

                case RetentionUnit.Days:
                    nudRetention.Minimum = 1;
                    nudRetention.Maximum = 30;
                    break;

                case RetentionUnit.Months:
                    nudRetention.Minimum = 1;
                    nudRetention.Maximum = 12;
                    break;
            }
            UpdateRetentionStatus();
        }

        private void InitializeTimeComboBox()
        {
            cmbRunTime.Items.Clear();

            for (int hour = 0; hour < 24; hour++)
            {
                DateTime time = DateTime.Today.AddHours(hour);

                cmbRunTime.Items.Add(
                    time.ToString("hh:mm tt"));
            }

            cmbRunTime.SelectedIndex = 2; // 02:00 AM
        }
        private void cmbFrequency_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateScheduleStatus();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = "Select the folder containing Print Job files.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            // If there is already a configured folder,
            // open the dialog at that location.
            if (Directory.Exists(txtPrintJobFolder.Text))
            {
                dialog.SelectedPath = txtPrintJobFolder.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPrintJobFolder.Text = dialog.SelectedPath;
            }
        }

        private void cmbRunTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateScheduleStatus();
        }
        private bool ValidateCleanupFolder()
        {
            string folder = txtPrintJobFolder.Text.Trim();

            if (string.IsNullOrWhiteSpace(folder))
            {
                MessageBox.Show(
                    "Please select a cleanup folder.",
                    "Cleanup Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!Directory.Exists(folder))
            {
                MessageBox.Show(
                    "The selected folder does not exist.",
                    "Cleanup Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!CanAccessFolder(folder))
            {
                MessageBox.Show(
                    "The selected folder cannot be accessed. " +
                    "Please check your permissions.",
                    "Cleanup Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }
            return true;
        }
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            if (!ValidateCleanupFolder())
                return;

            // Save settings...
        }
        private bool CanAccessFolder(string folder)
        {
            try
            {
                _ = Directory.EnumerateFileSystemEntries(folder)
                    .Take(1)
                    .ToList();

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void nudRetention_ValueChanged(object sender, EventArgs e)
        {
            UpdateRetentionStatus();
        }
    }
}
