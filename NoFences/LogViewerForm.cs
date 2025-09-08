using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NoFences.Util;

namespace NoFences
{
    public partial class LogViewerForm : Form
    {
        private TextBox logTextBox;
        private Button refreshButton;
        private Button clearButton;
        private Button saveButton;
        private CheckBox autoScrollCheckBox;
        private ComboBox logLevelComboBox;
        private Label statusLabel;
        
        private readonly string logFilePath;
        private readonly Logger logger;
        private DateTime lastUpdateTime;

        public LogViewerForm()
        {
            logger = Logger.Instance;
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BetterNoFences");
            logFilePath = Path.Combine(appDataPath, "application.log");
            
            InitializeComponent();
            LoadLogContent();
            
            // Auto-refresh timer
            var refreshTimer = new Timer();
            refreshTimer.Interval = 2000; // Refresh every 2 seconds
            refreshTimer.Tick += (s, e) => {
                if (autoScrollCheckBox.Checked)
                {
                    RefreshLogContent();
                }
            };
            refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = "NoFences - Log Viewer";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = this.Icon; // Use default form icon
            
            // Create controls
            var topPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                Padding = new Padding(5)
            };

            refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(5, 8),
                Size = new Size(75, 23),
                UseVisualStyleBackColor = true
            };
            refreshButton.Click += RefreshButton_Click;

            clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(85, 8),
                Size = new Size(75, 23),
                UseVisualStyleBackColor = true
            };
            clearButton.Click += ClearButton_Click;

            saveButton = new Button
            {
                Text = "Save As...",
                Location = new Point(165, 8),
                Size = new Size(75, 23),
                UseVisualStyleBackColor = true
            };
            saveButton.Click += SaveButton_Click;

            autoScrollCheckBox = new CheckBox
            {
                Text = "Auto-scroll",
                Location = new Point(250, 10),
                Size = new Size(80, 20),
                Checked = true,
                UseVisualStyleBackColor = true
            };

            var logLevelLabel = new Label
            {
                Text = "Level:",
                Location = new Point(340, 12),
                Size = new Size(40, 15)
            };

            logLevelComboBox = new ComboBox
            {
                Location = new Point(385, 8),
                Size = new Size(80, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            logLevelComboBox.Items.AddRange(new[] { "All", "Debug", "Info", "Warning", "Error", "Critical" });
            logLevelComboBox.SelectedIndex = 0;
            logLevelComboBox.SelectedIndexChanged += LogLevelComboBox_SelectedIndexChanged;

            var bottomPanel = new Panel
            {
                Height = 25,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5, 2, 5, 2)
            };

            statusLabel = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            logTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LightGray
            };

            // Add controls to panels
            topPanel.Controls.AddRange(new Control[] { refreshButton, clearButton, saveButton, autoScrollCheckBox, logLevelLabel, logLevelComboBox });
            bottomPanel.Controls.Add(statusLabel);

            // Add panels to form
            this.Controls.Add(logTextBox);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void LoadLogContent()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    var content = File.ReadAllText(logFilePath);
                    FilterAndDisplayLogs(content);
                    
                    var fileInfo = new FileInfo(logFilePath);
                    lastUpdateTime = fileInfo.LastWriteTime;
                    statusLabel.Text = $"Log loaded - {fileInfo.Length} bytes - Last updated: {lastUpdateTime:HH:mm:ss}";
                }
                else
                {
                    logTextBox.Text = "No log file found.";
                    statusLabel.Text = "No log file found";
                }
            }
            catch (Exception ex)
            {
                logTextBox.Text = $"Error loading log file: {ex.Message}";
                statusLabel.Text = "Error loading log";
            }
        }

        private void RefreshLogContent()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    if (fileInfo.LastWriteTime > lastUpdateTime)
                    {
                        LoadLogContent();
                        
                        if (autoScrollCheckBox.Checked)
                        {
                            logTextBox.SelectionStart = logTextBox.Text.Length;
                            logTextBox.ScrollToCaret();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error refreshing: {ex.Message}";
            }
        }

        private void FilterAndDisplayLogs(string content)
        {
            var selectedLevel = logLevelComboBox.SelectedItem?.ToString() ?? "All";
            
            if (selectedLevel == "All")
            {
                logTextBox.Text = content;
                return;
            }

            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var filteredLines = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    filteredLines.AppendLine(line);
                    continue;
                }

                // Check if line contains the selected log level
                if (line.Contains($"[{selectedLevel.ToUpper().PadRight(8)}]"))
                {
                    filteredLines.AppendLine(line);
                }
            }

            logTextBox.Text = filteredLines.ToString();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadLogContent();
            if (autoScrollCheckBox.Checked)
            {
                logTextBox.SelectionStart = logTextBox.Text.Length;
                logTextBox.ScrollToCaret();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will clear the log file. Are you sure?",
                "Clear Log",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    File.WriteAllText(logFilePath, string.Empty);
                    logTextBox.Clear();
                    statusLabel.Text = "Log cleared";
                    logger.Info("Log file cleared by user", "LogViewer");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*";
                saveDialog.DefaultExt = "txt";
                saveDialog.FileName = $"NoFences_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FileName, logTextBox.Text);
                        statusLabel.Text = $"Log saved to: {saveDialog.FileName}";
                        logger.Info($"Log exported to: {saveDialog.FileName}", "LogViewer");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving log: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LogLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(logFilePath))
            {
                var content = File.ReadAllText(logFilePath);
                FilterAndDisplayLogs(content);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Hide instead of closing when user clicks X
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }
    }
}