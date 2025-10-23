using DarkUI.Controls;
using DarkUI.Forms;
using Fenceless.Model;
using Fenceless.Util;
using Fenceless.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Fenceless.UI
{
    public partial class SettingsForm : Form
    {
        private readonly Logger logger;
        private List<FenceInfo> fenceInfos;
        private FenceInfo selectedFenceInfo;
        private bool isUpdatingControls = false;

        public SettingsForm()
        {
            logger = Logger.Instance;
            logger.Debug("Creating settings form", "SettingsForm");

            InitializeComponent();
            LoadSettings();
            LoadFences();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "Fenceless Settings";
            this.Size = new Size(1200, 700);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(800, 600);

            CreateControls();
            SetupEventHandlers();

            this.ResumeLayout(false);
        }

        #region Control Declarations

        // Main layout
        private TabControl mainTabControl;
        private TabPage globalTab;
        private TabPage defaultsTab;
        private TabPage fencesTab;

        // Global Settings Controls
        private DarkCheckBox chkAutoSave;
        private DarkNumericUpDown nudAutoSaveInterval;
        private DarkCheckBox chkShowTooltips;
        private DarkCheckBox chkEnableAnimations;
        private DarkCheckBox chkStartWithWindows;
        private DarkComboBox cmbLogLevel;
        private DarkCheckBox chkEnableFileLogging;
        private DarkTextBox txtToggleTransparencyShortcut;
        private DarkTextBox txtToggleAutoHideShortcut;
        private DarkTextBox txtShowAllFencesShortcut;
        private DarkTextBox txtCreateNewFenceShortcut;
        private DarkTextBox txtOpenSettingsShortcut;
        private DarkTextBox txtToggleLockShortcut;
        private DarkTextBox txtMinimizeAllFencesShortcut;
        private DarkTextBox txtRefreshFencesShortcut;

        // Default Settings Controls
        private DarkNumericUpDown nudDefaultFenceWidth;
        private DarkNumericUpDown nudDefaultFenceHeight;
        private DarkNumericUpDown nudDefaultTitleHeight;
        private DarkNumericUpDown nudDefaultTransparency;
        private DarkCheckBox chkDefaultAutoHide;
        private DarkNumericUpDown nudDefaultAutoHideDelay;
        private DarkButton btnDefaultBackgroundColor;
        private DarkNumericUpDown nudDefaultBackgroundTransparency;
        private DarkButton btnDefaultTitleBackgroundColor;
        private DarkNumericUpDown nudDefaultTitleBackgroundTransparency;
        private DarkButton btnDefaultTextColor;
        private DarkNumericUpDown nudDefaultTextTransparency;
        private DarkButton btnDefaultBorderColor;
        private DarkNumericUpDown nudDefaultBorderTransparency;
        private DarkNumericUpDown nudDefaultBorderWidth;
        private DarkNumericUpDown nudDefaultCornerRadius;
        private DarkCheckBox chkDefaultShowShadow;
        private DarkComboBox cmbDefaultIconSize;
        private DarkNumericUpDown nudDefaultItemSpacing;

        // Fence Management Controls
        private ListBox lstFences;
        private DarkGroupBox grpFenceSettings;
        private DarkTextBox txtFenceName;
        private DarkNumericUpDown nudFenceTransparency;
        private DarkCheckBox chkFenceAutoHide;
        private DarkNumericUpDown nudFenceAutoHideDelay;
        private DarkCheckBox chkFenceLocked;
        private DarkCheckBox chkFenceCanMinify;
        private DarkNumericUpDown nudFenceWidth;
        private DarkNumericUpDown nudFenceHeight;
        private DarkNumericUpDown nudFenceTitleHeight;
        private DarkButton btnFenceBackgroundColor;
        private DarkNumericUpDown nudFenceBackgroundTransparency;
        private DarkButton btnFenceTitleBackgroundColor;
        private DarkNumericUpDown nudFenceTitleBackgroundTransparency;
        private DarkButton btnFenceTextColor;
        private DarkNumericUpDown nudFenceTextTransparency;
        private DarkButton btnFenceBorderColor;
        private DarkNumericUpDown nudFenceBorderTransparency;
        private DarkNumericUpDown nudFenceBorderWidth;
        private DarkNumericUpDown nudFenceCornerRadius;
        private DarkCheckBox chkFenceShowShadow;
        private DarkComboBox cmbFenceIconSize;
        private DarkNumericUpDown nudFenceItemSpacing;

        // Buttons
        private DarkButton btnOK;
        private DarkButton btnCancel;
        private DarkButton btnApply;
        private DarkButton btnRefreshFences;
        private DarkButton btnHighlightFence;
        private DarkButton btnAddFence;
        private DarkButton btnResetToDefaults;
        private DarkButton btnSetAsDefaults;

        #endregion

        private void CreateControls()
        {
            // Create main tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            // Create tabs
            globalTab = new TabPage("Global Settings")
            {
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            defaultsTab = new TabPage("Default Settings")
            {
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            fencesTab = new TabPage("Fence Management")
            {
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            mainTabControl.TabPages.AddRange(new[] { globalTab, defaultsTab, fencesTab });

            CreateGlobalSettingsTab();
            CreateDefaultSettingsTab();
            CreateFenceManagementTab();
            CreateButtonPanel();

            this.Controls.Add(mainTabControl);
        }

        private void CreateGlobalSettingsTab()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            // Application Settings Group
            var grpApp = new DarkGroupBox
            {
                Text = "Application Settings",
                Location = new Point(10, 10),
                Size = new Size(450, 150)
            };

            chkAutoSave = new DarkCheckBox
            {
                Text = "Auto Save",
                Location = new Point(10, 25),
                AutoSize = true
            };

            var lblAutoSaveInterval = new DarkLabel
            {
                Text = "Auto Save Interval (seconds):",
                Location = new Point(10, 55),
                AutoSize = true
            };

            nudAutoSaveInterval = new DarkNumericUpDown
            {
                Location = new Point(200, 52),
                Width = 100,
                Minimum = 5,
                Maximum = 300
            };

            chkShowTooltips = new DarkCheckBox
            {
                Text = "Show Tooltips",
                Location = new Point(10, 85),
                AutoSize = true
            };

            chkEnableAnimations = new DarkCheckBox
            {
                Text = "Enable Animations",
                Location = new Point(10, 115),
                AutoSize = true
            };

            chkStartWithWindows = new DarkCheckBox
            {
                Text = "Start with Windows",
                Location = new Point(200, 25),
                AutoSize = true
            };

            grpApp.Controls.AddRange(new Control[] {
                chkAutoSave, lblAutoSaveInterval, nudAutoSaveInterval,
                chkShowTooltips, chkEnableAnimations, chkStartWithWindows
            });

            // Logging Settings Group
            var grpLogging = new DarkGroupBox
            {
                Text = "Logging Settings",
                Location = new Point(10, 170),
                Size = new Size(450, 100)
            };

            var lblLogLevel = new DarkLabel
            {
                Text = "Log Level:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            cmbLogLevel = new DarkComboBox
            {
                Location = new Point(100, 22),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLogLevel.Items.AddRange(new[] { "Debug", "Info", "Warning", "Error", "Critical" });

            chkEnableFileLogging = new DarkCheckBox
            {
                Text = "Enable File Logging",
                Location = new Point(10, 55),
                AutoSize = true
            };

            grpLogging.Controls.AddRange(new Control[] {
                lblLogLevel, cmbLogLevel, chkEnableFileLogging
            });

            // Global Hotkeys Group
            var grpHotkeys = new DarkGroupBox
            {
                Text = "Global Hotkeys",
                Location = new Point(480, 10),
                Size = new Size(400, 260)
            };

            CreateHotkeyControls(grpHotkeys);

            scrollPanel.Controls.AddRange(new Control[] { grpApp, grpLogging, grpHotkeys });
            globalTab.Controls.Add(scrollPanel);
        }

        private void CreateHotkeyControls(DarkGroupBox parent)
        {
            var hotkeys = new[]
            {
                ("Toggle Transparency:", "txtToggleTransparencyShortcut"),
                ("Toggle Auto-Hide:", "txtToggleAutoHideShortcut"),
                ("Show All Fences:", "txtShowAllFencesShortcut"),
                ("Create New Fence:", "txtCreateNewFenceShortcut"),
                ("Open Settings:", "txtOpenSettingsShortcut"),
                ("Toggle Lock:", "txtToggleLockShortcut"),
                ("Minimize All Fences:", "txtMinimizeAllFencesShortcut"),
                ("Refresh Fences:", "txtRefreshFencesShortcut")
            };

            int yPos = 25;
            foreach (var (label, controlName) in hotkeys)
            {
                var lbl = new DarkLabel
                {
                    Text = label,
                    Location = new Point(10, yPos + 3),
                    Size = new Size(150, 20)
                };

                var txt = new DarkTextBox
                {
                    Location = new Point(170, yPos),
                    Width = 150,
                    ReadOnly = true
                };

                var btnClear = new DarkButton
                {
                    Text = "Clear",
                    Location = new Point(330, yPos),
                    Size = new Size(50, 23)
                };

                btnClear.Click += (s, e) => txt.Text = "";

                // Assign to the appropriate field
                switch (controlName)
                {
                    case "txtToggleTransparencyShortcut": txtToggleTransparencyShortcut = txt; break;
                    case "txtToggleAutoHideShortcut": txtToggleAutoHideShortcut = txt; break;
                    case "txtShowAllFencesShortcut": txtShowAllFencesShortcut = txt; break;
                    case "txtCreateNewFenceShortcut": txtCreateNewFenceShortcut = txt; break;
                    case "txtOpenSettingsShortcut": txtOpenSettingsShortcut = txt; break;
                    case "txtToggleLockShortcut": txtToggleLockShortcut = txt; break;
                    case "txtMinimizeAllFencesShortcut": txtMinimizeAllFencesShortcut = txt; break;
                    case "txtRefreshFencesShortcut": txtRefreshFencesShortcut = txt; break;
                }

                parent.Controls.AddRange(new Control[] { lbl, txt, btnClear });
                yPos += 30;
            }
        }

        private void CreateDefaultSettingsTab()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            // Size Settings Group
            var grpSize = new DarkGroupBox
            {
                Text = "Default Size Settings",
                Location = new Point(10, 10),
                Size = new Size(400, 150)
            };

            CreateLabeledNumericUpDown(grpSize, "Width:", out nudDefaultFenceWidth, 200, 2000, 10, 25);
            CreateLabeledNumericUpDown(grpSize, "Height:", out nudDefaultFenceHeight, 200, 2000, 10, 55);
            CreateLabeledNumericUpDown(grpSize, "Title Height:", out nudDefaultTitleHeight, 16, 100, 10, 85);

            // Appearance Settings Group
            var grpAppearance = new DarkGroupBox
            {
                Text = "Default Appearance Settings",
                Location = new Point(10, 170),
                Size = new Size(400, 150)
            };

            CreateLabeledNumericUpDown(grpAppearance, "Transparency (%):", out nudDefaultTransparency, 25, 100, 10, 25);

            chkDefaultAutoHide = new DarkCheckBox
            {
                Text = "Auto Hide",
                Location = new Point(10, 55),
                AutoSize = true
            };

            CreateLabeledNumericUpDown(grpAppearance, "Auto Hide Delay (ms):", out nudDefaultAutoHideDelay, 500, 10000, 10, 85);

            grpAppearance.Controls.Add(chkDefaultAutoHide);

            // Color Settings Group
            var grpColors = new DarkGroupBox
            {
                Text = "Default Color Settings",
                Location = new Point(420, 10),
                Size = new Size(450, 310)
            };

            CreateColorSetting(grpColors, "Background Color:", out btnDefaultBackgroundColor, out nudDefaultBackgroundTransparency, 10, 25);
            CreateColorSetting(grpColors, "Title Background Color:", out btnDefaultTitleBackgroundColor, out nudDefaultTitleBackgroundTransparency, 10, 85);
            CreateColorSetting(grpColors, "Text Color:", out btnDefaultTextColor, out nudDefaultTextTransparency, 10, 145);
            CreateColorSetting(grpColors, "Border Color:", out btnDefaultBorderColor, out nudDefaultBorderTransparency, 10, 205);

            // Style Settings Group
            var grpStyle = new DarkGroupBox
            {
                Text = "Default Style Settings",
                Location = new Point(10, 330),
                Size = new Size(400, 200)
            };

            CreateLabeledNumericUpDown(grpStyle, "Border Width:", out nudDefaultBorderWidth, 0, 10, 10, 25);
            CreateLabeledNumericUpDown(grpStyle, "Corner Radius:", out nudDefaultCornerRadius, 0, 20, 10, 55);

            chkDefaultShowShadow = new DarkCheckBox
            {
                Text = "Show Shadow",
                Location = new Point(10, 85),
                AutoSize = true
            };

            var lblIconSize = new DarkLabel
            {
                Text = "Icon Size:",
                Location = new Point(10, 118),
                AutoSize = true
            };

            cmbDefaultIconSize = new DarkComboBox
            {
                Location = new Point(120, 115),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDefaultIconSize.Items.AddRange(new[] { "16", "24", "32", "48", "64" });

            CreateLabeledNumericUpDown(grpStyle, "Item Spacing:", out nudDefaultItemSpacing, 5, 50, 10, 145);

            grpStyle.Controls.AddRange(new Control[] { chkDefaultShowShadow, lblIconSize, cmbDefaultIconSize });

            scrollPanel.Controls.AddRange(new Control[] { grpSize, grpAppearance, grpColors, grpStyle });
            defaultsTab.Controls.Add(scrollPanel);
        }

        private void CreateFenceManagementTab()
        {
            // Main container with proper layout
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(60, 63, 65)
            };
            
            // Set column styles - left column wider width, right column fills remaining space
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Left panel container for fence list and buttons
            var leftContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 63, 65),
                Padding = new Padding(5)
            };

            // Fence list group box
            var fenceListGroup = new DarkGroupBox
            {
                Text = "Active Fences",
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // Container for buttons and list with proper layout - buttons first (higher)
            var listContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(60, 63, 65)
            };
            
            // Set row styles - buttons at top with fixed height, list takes remaining space
            listContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            listContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            listContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Button panel with proper layout - moved to top
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                BackColor = Color.FromArgb(60, 63, 65),
                Margin = new Padding(0, 0, 0, 5)
            };
            
            // Equal width columns for buttons
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            buttonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Create buttons with proper sizing
            btnRefreshFences = new DarkButton
            {
                Text = "Refresh",
                Dock = DockStyle.Fill,
                Margin = new Padding(1)
            };

            btnHighlightFence = new DarkButton
            {
                Text = "Highlight",
                Dock = DockStyle.Fill,
                Margin = new Padding(1)
            };

            btnAddFence = new DarkButton
            {
                Text = "Add",
                Dock = DockStyle.Fill,
                Margin = new Padding(1)
            };

            // Add buttons to button panel
            buttonPanel.Controls.Add(btnRefreshFences, 0, 0);
            buttonPanel.Controls.Add(btnHighlightFence, 1, 0);
            buttonPanel.Controls.Add(btnAddFence, 2, 0);

            // Fence list - moved below buttons
            lstFences = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                Margin = new Padding(0)
            };
            lstFences.DisplayMember = "Name";

            // Add buttons and list to list container - buttons first (row 0), then list (row 1)
            listContainer.Controls.Add(buttonPanel, 0, 0);
            listContainer.Controls.Add(lstFences, 0, 1);

            // Add list container to group box
            fenceListGroup.Controls.Add(listContainer);
            leftContainer.Controls.Add(fenceListGroup);

            // Right panel - Fence settings
            grpFenceSettings = new DarkGroupBox
            {
                Text = "Fence Settings",
                Dock = DockStyle.Fill,
                Enabled = false,
                Margin = new Padding(5, 0, 0, 0)
            };

            CreateFenceSettingsControls();

            // Add panels to main container
            mainContainer.Controls.Add(leftContainer, 0, 0);
            mainContainer.Controls.Add(grpFenceSettings, 1, 0);
            
            // Add main container to tab
            fencesTab.Controls.Add(mainContainer);
        }

        private void CreateFenceSettingsControls()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65),
                Padding = new Padding(10)
            };

            // Basic Settings Group
            var grpBasic = new DarkGroupBox
            {
                Text = "Basic Settings",
                Location = new Point(0, 0),
                Size = new Size(400, 220)
            };

            var lblName = new DarkLabel
            {
                Text = "Name:",
                Location = new Point(10, 25),
                AutoSize = true
            };

            txtFenceName = new DarkTextBox
            {
                Location = new Point(120, 22),
                Width = 200
            };

            CreateLabeledNumericUpDown(grpBasic, "Transparency (%):", out nudFenceTransparency, 25, 100, 10, 55);

            chkFenceAutoHide = new DarkCheckBox
            {
                Text = "Auto Hide",
                Location = new Point(10, 85),
                AutoSize = true
            };

            CreateLabeledNumericUpDown(grpBasic, "Auto Hide Delay (ms):", out nudFenceAutoHideDelay, 500, 10000, 10, 115);

            chkFenceLocked = new DarkCheckBox
            {
                Text = "Locked",
                Location = new Point(10, 145),
                AutoSize = true
            };

            chkFenceCanMinify = new DarkCheckBox
            {
                Text = "Can Minify",
                Location = new Point(150, 145),
                AutoSize = true
            };

            grpBasic.Controls.AddRange(new Control[] {
                lblName, txtFenceName, chkFenceAutoHide, chkFenceLocked, chkFenceCanMinify
            });

            // Size Settings Group
            var grpFenceSize = new DarkGroupBox
            {
                Text = "Size Settings",
                Location = new Point(0, 230),
                Size = new Size(400, 120)
            };

            CreateLabeledNumericUpDown(grpFenceSize, "Width:", out nudFenceWidth, 200, 2000, 10, 25);
            CreateLabeledNumericUpDown(grpFenceSize, "Height:", out nudFenceHeight, 200, 2000, 10, 55);
            CreateLabeledNumericUpDown(grpFenceSize, "Title Height:", out nudFenceTitleHeight, 16, 100, 10, 85);

            // Color Settings Group
            var grpFenceColors = new DarkGroupBox
            {
                Text = "Color Settings",
                Location = new Point(410, 0),
                Size = new Size(450, 350)
            };

            CreateColorSetting(grpFenceColors, "Background Color:", out btnFenceBackgroundColor, out nudFenceBackgroundTransparency, 10, 25);
            CreateColorSetting(grpFenceColors, "Title Background Color:", out btnFenceTitleBackgroundColor, out nudFenceTitleBackgroundTransparency, 10, 85);
            CreateColorSetting(grpFenceColors, "Text Color:", out btnFenceTextColor, out nudFenceTextTransparency, 10, 145);
            CreateColorSetting(grpFenceColors, "Border Color:", out btnFenceBorderColor, out nudFenceBorderTransparency, 10, 205);

            // Style Settings Group
            var grpFenceStyle = new DarkGroupBox
            {
                Text = "Style Settings",
                Location = new Point(410, 360),
                Size = new Size(450, 200)
            };

            CreateLabeledNumericUpDown(grpFenceStyle, "Border Width:", out nudFenceBorderWidth, 0, 10, 10, 25);
            CreateLabeledNumericUpDown(grpFenceStyle, "Corner Radius:", out nudFenceCornerRadius, 0, 20, 10, 55);

            chkFenceShowShadow = new DarkCheckBox
            {
                Text = "Show Shadow",
                Location = new Point(10, 85),
                AutoSize = true
            };

            var lblFenceIconSize = new DarkLabel
            {
                Text = "Icon Size:",
                Location = new Point(10, 118),
                AutoSize = true
            };

            cmbFenceIconSize = new DarkComboBox
            {
                Location = new Point(120, 115),
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFenceIconSize.Items.AddRange(new[] { "16", "24", "32", "48", "64" });

            CreateLabeledNumericUpDown(grpFenceStyle, "Item Spacing:", out nudFenceItemSpacing, 5, 50, 10, 145);

            grpFenceStyle.Controls.AddRange(new Control[] { chkFenceShowShadow, lblFenceIconSize, cmbFenceIconSize });

            // Action buttons
            btnResetToDefaults = new DarkButton
            {
                Text = "Reset to Defaults",
                Location = new Point(10, 570),
                Size = new Size(150, 30)
            };

            btnSetAsDefaults = new DarkButton
            {
                Text = "Set as Defaults",
                Location = new Point(170, 570),
                Size = new Size(150, 30)
            };

            scrollPanel.Controls.AddRange(new Control[] {
                grpBasic, grpFenceSize, grpFenceColors, grpFenceStyle,
                btnResetToDefaults, btnSetAsDefaults
            });

            grpFenceSettings.Controls.Add(scrollPanel);
        }

        private void CreateButtonPanel()
        {
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            btnApply = new DarkButton
            {
                Text = "Apply",
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnApply.Location = new Point(buttonPanel.Width - 260, 10);

            btnOK = new DarkButton
            {
                Text = "OK",
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };
            btnOK.Location = new Point(buttonPanel.Width - 170, 10);

            btnCancel = new DarkButton
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Location = new Point(buttonPanel.Width - 80, 10);

            buttonPanel.Controls.AddRange(new Control[] { btnApply, btnOK, btnCancel });
            this.Controls.Add(buttonPanel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void CreateLabeledNumericUpDown(Control parent, string labelText, out DarkNumericUpDown numericUpDown,
            decimal min, decimal max, int x, int y)
        {
            var label = new DarkLabel
            {
                Text = labelText,
                Location = new Point(x, y + 3),
                Size = new Size(110, 20)
            };

            numericUpDown = new DarkNumericUpDown
            {
                Location = new Point(x + 120, y),
                Width = 100,
                Minimum = min,
                Maximum = max
            };

            parent.Controls.AddRange(new Control[] { label, numericUpDown });
        }

        private void CreateColorSetting(Control parent, string labelText, out DarkButton colorButton,
            out DarkNumericUpDown transparencyUpDown, int x, int y)
        {
            var label = new DarkLabel
            {
                Text = labelText,
                Location = new Point(x, y + 3),
                Size = new Size(130, 20)
            };

            colorButton = new DarkButton
            {
                Text = "Choose Color",
                Location = new Point(x + 140, y),
                Size = new Size(120, 23)
            };

            var transparencyLabel = new DarkLabel
            {
                Text = "Transparency (%):",
                Location = new Point(x + 270, y + 3),
                Size = new Size(100, 20)
            };

            transparencyUpDown = new DarkNumericUpDown
            {
                Location = new Point(x + 380, y),
                Width = 60,
                Minimum = 0,
                Maximum = 100
            };

            parent.Controls.AddRange(new Control[] { label, colorButton, transparencyLabel, transparencyUpDown });
        }

        private void SetupEventHandlers()
        {
            // Global settings auto-apply handlers
            chkAutoSave.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudAutoSaveInterval.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            chkShowTooltips.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            chkEnableAnimations.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            cmbLogLevel.SelectedIndexChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            chkEnableFileLogging.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };

            // Hotkey handlers
            txtToggleTransparencyShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtToggleAutoHideShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtShowAllFencesShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtCreateNewFenceShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtOpenSettingsShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtToggleLockShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtMinimizeAllFencesShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            txtRefreshFencesShortcut.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };

            // Default settings auto-apply handlers
            nudDefaultFenceWidth.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultFenceHeight.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultTitleHeight.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            chkDefaultAutoHide.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultAutoHideDelay.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultBackgroundTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultTitleBackgroundTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultTextTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultBorderTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultBorderWidth.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultCornerRadius.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            chkDefaultShowShadow.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            cmbDefaultIconSize.SelectedIndexChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };
            nudDefaultItemSpacing.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyGlobalSettings(); };

            // Color button handlers
            btnDefaultBackgroundColor.Click += (s, e) => ShowColorDialog(btnDefaultBackgroundColor, "DefaultBackgroundColor");
            btnDefaultTitleBackgroundColor.Click += (s, e) => ShowColorDialog(btnDefaultTitleBackgroundColor, "DefaultTitleBackgroundColor");
            btnDefaultTextColor.Click += (s, e) => ShowColorDialog(btnDefaultTextColor, "DefaultTextColor");
            btnDefaultBorderColor.Click += (s, e) => ShowColorDialog(btnDefaultBorderColor, "DefaultBorderColor");

            btnFenceBackgroundColor.Click += (s, e) => ShowColorDialog(btnFenceBackgroundColor, "BackgroundColor", true);
            btnFenceTitleBackgroundColor.Click += (s, e) => ShowColorDialog(btnFenceTitleBackgroundColor, "TitleBackgroundColor", true);
            btnFenceTextColor.Click += (s, e) => ShowColorDialog(btnFenceTextColor, "TextColor", true);
            btnFenceBorderColor.Click += (s, e) => ShowColorDialog(btnFenceBorderColor, "BorderColor", true);

            // Fence management handlers
            lstFences.DrawItem += LstFences_DrawItem;
            lstFences.SelectedIndexChanged += LstFences_SelectedIndexChanged;
            btnRefreshFences.Click += (s, e) => LoadFences();
            btnHighlightFence.Click += BtnHighlight_Click;
            btnAddFence.Click += BtnAdd_Click;
            btnResetToDefaults.Click += BtnResetToDefaults_Click;
            btnSetAsDefaults.Click += BtnSetAsDefaults_Click;

            // Fence settings auto-apply handlers
            txtFenceName.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            chkFenceAutoHide.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceAutoHideDelay.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            chkFenceLocked.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            chkFenceCanMinify.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceWidth.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceHeight.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceTitleHeight.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceBackgroundTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceTitleBackgroundTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceTextTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceBorderTransparency.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceBorderWidth.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceCornerRadius.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            chkFenceShowShadow.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            cmbFenceIconSize.SelectedIndexChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            nudFenceItemSpacing.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };

            // Main buttons
            btnOK.Click += BtnOK_Click;
            btnApply.Click += BtnApply_Click;
        }

        private void LoadSettings()
        {
            try
            {
                isUpdatingControls = true;
                logger.Debug("Loading settings into form", "SettingsForm");
                var settings = AppSettings.Instance;

                // Global settings
                chkAutoSave.Checked = settings.AutoSave;
                nudAutoSaveInterval.Value = settings.AutoSaveInterval;
                chkShowTooltips.Checked = settings.ShowTooltips;
                chkEnableAnimations.Checked = settings.EnableAnimations;
                chkStartWithWindows.Checked = settings.StartWithWindows;
                cmbLogLevel.SelectedItem = settings.LogLevel;
                chkEnableFileLogging.Checked = settings.EnableFileLogging;

                // Hotkeys
                txtToggleTransparencyShortcut.Text = settings.ToggleTransparencyShortcut;
                txtToggleAutoHideShortcut.Text = settings.ToggleAutoHideShortcut;
                txtShowAllFencesShortcut.Text = settings.ShowAllFencesShortcut;
                txtCreateNewFenceShortcut.Text = settings.CreateNewFenceShortcut;
                txtOpenSettingsShortcut.Text = settings.OpenSettingsShortcut;
                txtToggleLockShortcut.Text = settings.ToggleLockShortcut;
                txtMinimizeAllFencesShortcut.Text = settings.MinimizeAllFencesShortcut;
                txtRefreshFencesShortcut.Text = settings.RefreshFencesShortcut;

                // Default settings
                nudDefaultFenceWidth.Value = settings.DefaultFenceWidth;
                nudDefaultFenceHeight.Value = settings.DefaultFenceHeight;
                nudDefaultTitleHeight.Value = settings.DefaultTitleHeight;
                nudDefaultTransparency.Value = settings.DefaultTransparency;
                chkDefaultAutoHide.Checked = settings.DefaultAutoHide;
                nudDefaultAutoHideDelay.Value = settings.DefaultAutoHideDelay;

                SetColorButton(btnDefaultBackgroundColor, settings.DefaultBackgroundColor);
                nudDefaultBackgroundTransparency.Value = settings.DefaultBackgroundTransparency;
                SetColorButton(btnDefaultTitleBackgroundColor, settings.DefaultTitleBackgroundColor);
                nudDefaultTitleBackgroundTransparency.Value = settings.DefaultTitleBackgroundTransparency;
                SetColorButton(btnDefaultTextColor, settings.DefaultTextColor);
                nudDefaultTextTransparency.Value = settings.DefaultTextTransparency;
                SetColorButton(btnDefaultBorderColor, settings.DefaultBorderColor);
                nudDefaultBorderTransparency.Value = settings.DefaultBorderTransparency;

                nudDefaultBorderWidth.Value = settings.DefaultBorderWidth;
                nudDefaultCornerRadius.Value = settings.DefaultCornerRadius;
                chkDefaultShowShadow.Checked = settings.DefaultShowShadow;
                cmbDefaultIconSize.SelectedItem = settings.DefaultIconSize.ToString();
                nudDefaultItemSpacing.Value = settings.DefaultItemSpacing;

                logger.Info("Settings loaded successfully", "SettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load settings", "SettingsForm", ex);
            }
            finally
            {
                isUpdatingControls = false;
            }
        }

        private void LoadFences()
        {
            try
            {
                logger.Debug("Loading fences into list", "SettingsForm");

                fenceInfos = FenceManager.Instance.GetAllFenceInfos();

                Guid? selectedId = selectedFenceInfo?.Id;

                lstFences.Items.Clear();

                foreach (var fence in fenceInfos)
                {
                    lstFences.Items.Add(fence);
                }

                if (selectedId.HasValue)
                {
                    var fenceToSelect = fenceInfos.FirstOrDefault(f => f.Id == selectedId.Value);
                    if (fenceToSelect != null)
                    {
                        lstFences.SelectedItem = fenceToSelect;
                    }
                }

                if (lstFences.SelectedItem == null && fenceInfos.Any())
                {
                    lstFences.SelectedItem = fenceInfos.First();
                }

                logger.Info($"Loaded {fenceInfos.Count} fences", "SettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load fences", "SettingsForm", ex);
            }
        }

        private void LoadFenceSettings()
        {
            if (selectedFenceInfo == null) return;

            try
            {
                isUpdatingControls = true;
                logger.Debug($"Loading settings for fence '{selectedFenceInfo.Name}'", "SettingsForm");

                txtFenceName.Text = selectedFenceInfo.Name;
                nudFenceTransparency.Value = selectedFenceInfo.Transparency;
                chkFenceAutoHide.Checked = selectedFenceInfo.AutoHide;
                nudFenceAutoHideDelay.Value = selectedFenceInfo.AutoHideDelay;
                chkFenceLocked.Checked = selectedFenceInfo.Locked;
                chkFenceCanMinify.Checked = selectedFenceInfo.CanMinify;
                nudFenceWidth.Value = selectedFenceInfo.Width;
                nudFenceHeight.Value = selectedFenceInfo.Height;
                nudFenceTitleHeight.Value = selectedFenceInfo.TitleHeight;

                SetColorButton(btnFenceBackgroundColor, selectedFenceInfo.BackgroundColor);
                nudFenceBackgroundTransparency.Value = selectedFenceInfo.BackgroundTransparency;
                SetColorButton(btnFenceTitleBackgroundColor, selectedFenceInfo.TitleBackgroundColor);
                nudFenceTitleBackgroundTransparency.Value = selectedFenceInfo.TitleBackgroundTransparency;
                SetColorButton(btnFenceTextColor, selectedFenceInfo.TextColor);
                nudFenceTextTransparency.Value = selectedFenceInfo.TextTransparency;
                SetColorButton(btnFenceBorderColor, selectedFenceInfo.BorderColor);
                nudFenceBorderTransparency.Value = selectedFenceInfo.BorderTransparency;

                nudFenceBorderWidth.Value = selectedFenceInfo.BorderWidth;
                nudFenceCornerRadius.Value = selectedFenceInfo.CornerRadius;
                chkFenceShowShadow.Checked = selectedFenceInfo.ShowShadow;
                cmbFenceIconSize.SelectedItem = selectedFenceInfo.IconSize.ToString();
                nudFenceItemSpacing.Value = selectedFenceInfo.ItemSpacing;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load fence settings for '{selectedFenceInfo?.Name}'", "SettingsForm", ex);
            }
            finally
            {
                isUpdatingControls = false;
            }
        }

        private void ShowColorDialog(DarkButton button, string propertyName, bool isFenceProperty = false)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog(this) == DialogResult.OK)
                {
                    SetColorButton(button, colorDialog.Color.ToArgb());

                    if (isFenceProperty)
                    {
                        ApplyFenceSettings();
                    }
                    else
                    {
                        ApplyGlobalSettings();
                    }
                }
            }
        }

        private void SetColorButton(DarkButton button, int argbColor)
        {
            var color = Color.FromArgb(argbColor);
            button.BackColor = color;
            button.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void ApplyGlobalSettings()
        {
            if (isUpdatingControls) return;

            try
            {
                logger.Debug("Applying global settings", "SettingsForm");
                var settings = AppSettings.Instance;

                settings.AutoSave = chkAutoSave.Checked;
                settings.AutoSaveInterval = (int)nudAutoSaveInterval.Value;
                settings.ShowTooltips = chkShowTooltips.Checked;
                settings.EnableAnimations = chkEnableAnimations.Checked;
                settings.LogLevel = cmbLogLevel.SelectedItem?.ToString() ?? "Info";
                settings.EnableFileLogging = chkEnableFileLogging.Checked;
                
                // Handle startup with Windows setting
                bool previousStartupSetting = settings.StartWithWindows;
                settings.StartWithWindows = chkStartWithWindows.Checked;
                
                if (previousStartupSetting != settings.StartWithWindows)
                {
                    if (settings.StartWithWindows)
                    {
                        if (!StartupManager.EnableStartup())
                        {
                            logger.Error("Failed to enable startup", "SettingsForm");
                            MessageBox.Show("Failed to enable startup with Windows. Please check the logs for details.", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            settings.StartWithWindows = false;
                            chkStartWithWindows.Checked = false;
                        }
                    }
                    else
                    {
                        if (!StartupManager.DisableStartup())
                        {
                            logger.Error("Failed to disable startup", "SettingsForm");
                            MessageBox.Show("Failed to disable startup with Windows. Please check the logs for details.", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            settings.StartWithWindows = true;
                            chkStartWithWindows.Checked = true;
                        }
                    }
                }

                settings.ToggleTransparencyShortcut = txtToggleTransparencyShortcut.Text;
                settings.ToggleAutoHideShortcut = txtToggleAutoHideShortcut.Text;
                settings.ShowAllFencesShortcut = txtShowAllFencesShortcut.Text;
                settings.CreateNewFenceShortcut = txtCreateNewFenceShortcut.Text;
                settings.OpenSettingsShortcut = txtOpenSettingsShortcut.Text;
                settings.ToggleLockShortcut = txtToggleLockShortcut.Text;
                settings.MinimizeAllFencesShortcut = txtMinimizeAllFencesShortcut.Text;
                settings.RefreshFencesShortcut = txtRefreshFencesShortcut.Text;

                settings.DefaultFenceWidth = (int)nudDefaultFenceWidth.Value;
                settings.DefaultFenceHeight = (int)nudDefaultFenceHeight.Value;
                settings.DefaultTitleHeight = (int)nudDefaultTitleHeight.Value;
                settings.DefaultTransparency = (int)nudDefaultTransparency.Value;
                settings.DefaultAutoHide = chkDefaultAutoHide.Checked;
                settings.DefaultAutoHideDelay = (int)nudDefaultAutoHideDelay.Value;

                settings.DefaultBackgroundColor = btnDefaultBackgroundColor.BackColor.ToArgb();
                settings.DefaultBackgroundTransparency = (int)nudDefaultBackgroundTransparency.Value;
                settings.DefaultTitleBackgroundColor = btnDefaultTitleBackgroundColor.BackColor.ToArgb();
                settings.DefaultTitleBackgroundTransparency = (int)nudDefaultTitleBackgroundTransparency.Value;
                settings.DefaultTextColor = btnDefaultTextColor.BackColor.ToArgb();
                settings.DefaultTextTransparency = (int)nudDefaultTextTransparency.Value;
                settings.DefaultBorderColor = btnDefaultBorderColor.BackColor.ToArgb();
                settings.DefaultBorderTransparency = (int)nudDefaultBorderTransparency.Value;

                settings.DefaultBorderWidth = (int)nudDefaultBorderWidth.Value;
                settings.DefaultCornerRadius = (int)nudDefaultCornerRadius.Value;
                settings.DefaultShowShadow = chkDefaultShowShadow.Checked;
                if (int.TryParse(cmbDefaultIconSize.SelectedItem?.ToString(), out int iconSize))
                    settings.DefaultIconSize = iconSize;
                settings.DefaultItemSpacing = (int)nudDefaultItemSpacing.Value;

                settings.SaveSettings();

                logger.Info("Global settings applied successfully", "SettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply global settings", "SettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFenceSettings()
        {
            if (selectedFenceInfo == null || isUpdatingControls) return;

            try
            {
                selectedFenceInfo.Name = txtFenceName.Text;
                selectedFenceInfo.Transparency = (int)nudFenceTransparency.Value;
                selectedFenceInfo.AutoHide = chkFenceAutoHide.Checked;
                selectedFenceInfo.AutoHideDelay = (int)nudFenceAutoHideDelay.Value;
                selectedFenceInfo.Locked = chkFenceLocked.Checked;
                selectedFenceInfo.CanMinify = chkFenceCanMinify.Checked;
                selectedFenceInfo.Width = (int)nudFenceWidth.Value;
                selectedFenceInfo.Height = (int)nudFenceHeight.Value;
                selectedFenceInfo.TitleHeight = (int)nudFenceTitleHeight.Value;

                selectedFenceInfo.BackgroundColor = btnFenceBackgroundColor.BackColor.ToArgb();
                selectedFenceInfo.BackgroundTransparency = (int)nudFenceBackgroundTransparency.Value;
                selectedFenceInfo.TitleBackgroundColor = btnFenceTitleBackgroundColor.BackColor.ToArgb();
                selectedFenceInfo.TitleBackgroundTransparency = (int)nudFenceTitleBackgroundTransparency.Value;
                selectedFenceInfo.TextColor = btnFenceTextColor.BackColor.ToArgb();
                selectedFenceInfo.TextTransparency = (int)nudFenceTextTransparency.Value;
                selectedFenceInfo.BorderColor = btnFenceBorderColor.BackColor.ToArgb();
                selectedFenceInfo.BorderTransparency = (int)nudFenceBorderTransparency.Value;

                selectedFenceInfo.BorderWidth = (int)nudFenceBorderWidth.Value;
                selectedFenceInfo.CornerRadius = (int)nudFenceCornerRadius.Value;
                selectedFenceInfo.ShowShadow = chkFenceShowShadow.Checked;
                if (int.TryParse(cmbFenceIconSize.SelectedItem?.ToString(), out int iconSize))
                    selectedFenceInfo.IconSize = iconSize;
                selectedFenceInfo.ItemSpacing = (int)nudFenceItemSpacing.Value;

                FenceManager.Instance.UpdateFence(selectedFenceInfo);
                FenceManager.Instance.ApplySettingsToFence(selectedFenceInfo);



                logger.Info($"Applied settings to fence '{selectedFenceInfo.Name}'", "SettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to apply fence settings for '{selectedFenceInfo?.Name}'", "SettingsForm", ex);
            }
        }

        #region Event Handlers

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyGlobalSettings();
                if (selectedFenceInfo != null)
                {
                    ApplyFenceSettings();
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply settings on OK", "SettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyGlobalSettings();
                if (selectedFenceInfo != null)
                {
                    ApplyFenceSettings();
                }

                MessageBox.Show("Settings applied successfully!", "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply settings", "SettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHighlight_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                FenceManager.Instance.HighlightFence(selectedFenceInfo.Id);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog("New Fence", "Enter fence name:"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    FenceManager.Instance.CreateFence(dialog.InputText);
                    LoadFences();
                }
            }
        }

        private void BtnResetToDefaults_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                var result = MessageBox.Show(
                    $"Reset fence '{selectedFenceInfo.Name}' to default settings?",
                    "Reset to Defaults",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var settings = AppSettings.Instance;

                    selectedFenceInfo.Transparency = settings.DefaultTransparency;
                    selectedFenceInfo.AutoHide = settings.DefaultAutoHide;
                    selectedFenceInfo.AutoHideDelay = settings.DefaultAutoHideDelay;
                    selectedFenceInfo.Width = settings.DefaultFenceWidth;
                    selectedFenceInfo.Height = settings.DefaultFenceHeight;
                    selectedFenceInfo.TitleHeight = settings.DefaultTitleHeight;
                    selectedFenceInfo.BackgroundColor = settings.DefaultBackgroundColor;
                    selectedFenceInfo.TitleBackgroundColor = settings.DefaultTitleBackgroundColor;
                    selectedFenceInfo.TextColor = settings.DefaultTextColor;
                    selectedFenceInfo.BorderColor = settings.DefaultBorderColor;
                    selectedFenceInfo.BackgroundTransparency = settings.DefaultBackgroundTransparency;
                    selectedFenceInfo.TitleBackgroundTransparency = settings.DefaultTitleBackgroundTransparency;
                    selectedFenceInfo.TextTransparency = settings.DefaultTextTransparency;
                    selectedFenceInfo.BorderTransparency = settings.DefaultBorderTransparency;
                    selectedFenceInfo.BorderWidth = settings.DefaultBorderWidth;
                    selectedFenceInfo.CornerRadius = settings.DefaultCornerRadius;
                    selectedFenceInfo.ShowShadow = settings.DefaultShowShadow;
                    selectedFenceInfo.IconSize = settings.DefaultIconSize;
                    selectedFenceInfo.ItemSpacing = settings.DefaultItemSpacing;

                    LoadFenceSettings();
                    ApplyFenceSettings();
                }
            }
        }

        private void BtnSetAsDefaults_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                var result = MessageBox.Show(
                    $"Set fence '{selectedFenceInfo.Name}' settings as new defaults?",
                    "Set as Defaults",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var settings = AppSettings.Instance;

                    settings.DefaultTransparency = selectedFenceInfo.Transparency;
                    settings.DefaultAutoHide = selectedFenceInfo.AutoHide;
                    settings.DefaultAutoHideDelay = selectedFenceInfo.AutoHideDelay;
                    settings.DefaultFenceWidth = selectedFenceInfo.Width;
                    settings.DefaultFenceHeight = selectedFenceInfo.Height;
                    settings.DefaultTitleHeight = selectedFenceInfo.TitleHeight;
                    settings.DefaultBackgroundColor = selectedFenceInfo.BackgroundColor;
                    settings.DefaultTitleBackgroundColor = selectedFenceInfo.TitleBackgroundColor;
                    settings.DefaultTextColor = selectedFenceInfo.TextColor;
                    settings.DefaultBorderColor = selectedFenceInfo.BorderColor;
                    settings.DefaultBackgroundTransparency = selectedFenceInfo.BackgroundTransparency;
                    settings.DefaultTitleBackgroundTransparency = selectedFenceInfo.TitleBackgroundTransparency;
                    settings.DefaultTextTransparency = selectedFenceInfo.TextTransparency;
                    settings.DefaultBorderTransparency = selectedFenceInfo.BorderTransparency;
                    settings.DefaultBorderWidth = selectedFenceInfo.BorderWidth;
                    settings.DefaultCornerRadius = selectedFenceInfo.CornerRadius;
                    settings.DefaultShowShadow = selectedFenceInfo.ShowShadow;
                    settings.DefaultIconSize = selectedFenceInfo.IconSize;
                    settings.DefaultItemSpacing = selectedFenceInfo.ItemSpacing;

                    settings.SaveSettings();
                    LoadSettings(); // Reload default settings controls

                    MessageBox.Show("Default settings updated successfully!", "Defaults Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void LstFences_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var listBox = (ListBox)sender;
            var fence = (FenceInfo)listBox.Items[e.Index];

            // Background color
            Color backgroundColor = (e.State & DrawItemState.Selected) != 0
                ? Color.FromArgb(81, 81, 81) // Selected color
                : Color.FromArgb(60, 63, 65); // Normal color

            using (var backgroundBrush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }

            // Text color
            using (var textBrush = new SolidBrush(Color.FromArgb(220, 220, 220)))
            {
                e.Graphics.DrawString(fence.Name, e.Font, textBrush, e.Bounds, StringFormat.GenericDefault);
            }

            e.DrawFocusRectangle();
        }

        private void LstFences_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFenceInfo = lstFences.SelectedItem as FenceInfo;
            grpFenceSettings.Enabled = selectedFenceInfo != null;
            LoadFenceSettings();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                logger?.Debug("Disposing settings form", "SettingsForm");
            }
            base.Dispose(disposing);
        }
    }
}