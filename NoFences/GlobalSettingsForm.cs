using NoFences.Model;
using NoFences.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NoFences
{
    public partial class GlobalSettingsForm : Form
    {
        private readonly Logger logger;
        private List<FenceInfo> fenceInfos;
        private FenceInfo selectedFenceInfo;
        private bool isUpdatingControls = false;

        // Controls
        private TabControl mainTabControl;
        private TabPage tabGlobal;
        private TabPage tabDefaults;
        private TabPage tabFences;
        
        // Global settings controls
        private GroupBox grpLogging;
        private ComboBox cmbLogLevel;
        private CheckBox chkEnableFileLogging;
        private GroupBox grpHotkeys;
        private TextBox txtToggleTransparencyShortcut;
        private TextBox txtToggleAutoHideShortcut;
        private TextBox txtShowAllFencesShortcut;
        private TextBox txtCreateNewFenceShortcut;
        private TextBox txtOpenSettingsShortcut;
        private TextBox txtToggleLockShortcut;
        private TextBox txtMinimizeAllFencesShortcut;
        private TextBox txtRefreshFencesShortcut;
        private GroupBox grpApplication;
        private CheckBox chkAutoSave;
        private NumericUpDown numAutoSaveInterval;
        private CheckBox chkShowTooltips;
        private CheckBox chkEnableAnimations;
        
        // Default settings controls
        private GroupBox grpDefaultSize;
        private NumericUpDown numDefaultWidth;
        private NumericUpDown numDefaultHeight;
        private NumericUpDown numDefaultTitleHeight;
        private GroupBox grpDefaultAppearance;
        private NumericUpDown numDefaultTransparency;
        private CheckBox chkDefaultAutoHide;
        private NumericUpDown numDefaultAutoHideDelay;
        private GroupBox grpDefaultColors;
        private Button btnDefaultBackgroundColor;
        private Button btnDefaultTitleBackgroundColor;
        private Button btnDefaultTextColor;
        private Button btnDefaultBorderColor;
        private NumericUpDown numDefaultBackgroundTransparency;
        private NumericUpDown numDefaultTitleBackgroundTransparency;
        private NumericUpDown numDefaultTextTransparency;
        private NumericUpDown numDefaultBorderTransparency;
        private NumericUpDown numDefaultBorderWidth;
        private NumericUpDown numDefaultCornerRadius;
        private CheckBox chkDefaultShowShadow;
        private ComboBox cmbDefaultIconSize;
        private NumericUpDown numDefaultItemSpacing;
        
        // Fence management controls
        private GroupBox grpFenceList;
        private ListBox lstFences;
        private Button btnRefreshFences;
        private Button btnHighlightFence;
        private Button btnShowAllFences;
        private Button btnAddFence;
        private GroupBox grpFenceSettings;
        private Label lblFenceName;
        private TextBox txtFenceName;
        private Label lblFenceTransparency;
        private NumericUpDown numFenceTransparency;
        private CheckBox chkFenceAutoHide;
        private NumericUpDown numFenceAutoHideDelay;
        private CheckBox chkFenceLocked;
        private CheckBox chkFenceCanMinify;
        private Label lblFenceSize;
        private NumericUpDown numFenceWidth;
        private NumericUpDown numFenceHeight;
        private NumericUpDown numFenceTitleHeight;
        private GroupBox grpFenceColors;
        private Button btnFenceBackgroundColor;
        private Button btnFenceTitleBackgroundColor;
        private Button btnFenceTextColor;
        private Button btnFenceBorderColor;
        private NumericUpDown numFenceBackgroundTransparency;
        private NumericUpDown numFenceTitleBackgroundTransparency;
        private NumericUpDown numFenceTextTransparency;
        private NumericUpDown numFenceBorderTransparency;
        private NumericUpDown numFenceBorderWidth;
        private NumericUpDown numFenceCornerRadius;
        private CheckBox chkFenceShowShadow;
        private ComboBox cmbFenceIconSize;
        private NumericUpDown numFenceItemSpacing;
        private Button btnResetFenceToDefaults;
        private Button btnSetFenceAsDefaults;
        
        // Button separator
        private Panel pnlButtonSeparator;

        // Dialog buttons
        private Button btnOK;
        private Button btnCancel;
        private Button btnApply;
        private ToolTip toolTip;

        public GlobalSettingsForm()
        {
            logger = Logger.Instance;
            logger.Debug("Creating global settings form", "GlobalSettingsForm");
            
            InitializeComponent();
            LoadSettings();
            LoadFences();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form setup
            this.Text = "BetterNoFences - Global Settings";
            this.Size = new Size(1200, 750);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            
            CreateControls();
            LayoutControls();
            SetupEventHandlers();
            
            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Main tab control
            mainTabControl = new TabControl();
            tabGlobal = new TabPage("Global Settings");
            tabDefaults = new TabPage("Default Settings");
            tabFences = new TabPage("Fence Management");
            
            // Global settings controls
            grpLogging = new GroupBox { Text = "Logging" };
            cmbLogLevel = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbLogLevel.Items.AddRange(new[] { "Debug", "Info", "Warning", "Error", "Critical" });
            chkEnableFileLogging = new CheckBox { Text = "Enable file logging" };
            
            grpHotkeys = new GroupBox { Text = "Global Hotkeys" };
            txtToggleTransparencyShortcut = new TextBox { Size = new Size(120, 23) };
            txtToggleAutoHideShortcut = new TextBox { Size = new Size(120, 23) };
            txtShowAllFencesShortcut = new TextBox { Size = new Size(120, 23) };
            txtCreateNewFenceShortcut = new TextBox { Size = new Size(120, 23) };
            txtOpenSettingsShortcut = new TextBox { Size = new Size(120, 23) };
            txtToggleLockShortcut = new TextBox { Size = new Size(120, 23) };
            txtMinimizeAllFencesShortcut = new TextBox { Size = new Size(120, 23) };
            txtRefreshFencesShortcut = new TextBox { Size = new Size(120, 23) };
            
            grpApplication = new GroupBox { Text = "Application Settings" };
            chkAutoSave = new CheckBox { Text = "Auto save" };
            numAutoSaveInterval = new NumericUpDown { Minimum = 5, Maximum = 300, Value = 30 };
            chkShowTooltips = new CheckBox { Text = "Show tooltips" };
            chkEnableAnimations = new CheckBox { Text = "Enable animations" };
            
            // Default settings controls
            grpDefaultSize = new GroupBox { Text = "Default Size" };
            numDefaultWidth = new NumericUpDown { Minimum = 200, Maximum = 2000, Value = 300 };
            numDefaultHeight = new NumericUpDown { Minimum = 200, Maximum = 2000, Value = 300 };
            numDefaultTitleHeight = new NumericUpDown { Minimum = 16, Maximum = 100, Value = 35 };
            
            grpDefaultAppearance = new GroupBox { Text = "Default Appearance" };
            numDefaultTransparency = new NumericUpDown { Minimum = 25, Maximum = 100, Value = 100 };
            chkDefaultAutoHide = new CheckBox { Text = "Auto hide by default" };
            numDefaultAutoHideDelay = new NumericUpDown { Minimum = 500, Maximum = 10000, Value = 2000 };
            
            grpDefaultColors = new GroupBox { Text = "Default Colors" };
            btnDefaultBackgroundColor = new Button { Text = "Background", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnDefaultTitleBackgroundColor = new Button { Text = "Title Bg", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnDefaultTextColor = new Button { Text = "Text", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnDefaultBorderColor = new Button { Text = "Border", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            numDefaultBackgroundTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numDefaultTitleBackgroundTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 80, Size = new Size(50, 23) };
            numDefaultTextTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numDefaultBorderTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numDefaultBorderWidth = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 0 };
            numDefaultCornerRadius = new NumericUpDown { Minimum = 0, Maximum = 20, Value = 0 };
            chkDefaultShowShadow = new CheckBox { Text = "Show shadow" };
            cmbDefaultIconSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDefaultIconSize.Items.AddRange(new[] { "16", "24", "32", "48", "64" });
            numDefaultItemSpacing = new NumericUpDown { Minimum = 5, Maximum = 50, Value = 15 };
            
            // Fence management controls
            grpFenceList = new GroupBox { Text = "Active Fences" };
            lstFences = new ListBox { };
            btnRefreshFences = new Button { Text = "Refresh", Size = new Size(75, 23) };
            btnHighlightFence = new Button { Text = "Highlight", Size = new Size(75, 23) };
            btnShowAllFences = new Button { Text = "Show All", Size = new Size(75, 23) };
            btnAddFence = new Button { Text = "Add Fence", Size = new Size(85, 23) };
            
            grpFenceSettings = new GroupBox { Text = "Fence Settings" };
            lblFenceName = new Label { Text = "Name:" };
            txtFenceName = new TextBox { };
            lblFenceTransparency = new Label { Text = "Transparency:" };
            numFenceTransparency = new NumericUpDown { Minimum = 25, Maximum = 100, Value = 100 };
            chkFenceAutoHide = new CheckBox { Text = "Auto hide" };
            numFenceAutoHideDelay = new NumericUpDown { Minimum = 500, Maximum = 10000, Value = 2000 };
            chkFenceLocked = new CheckBox { Text = "Locked" };
            chkFenceCanMinify = new CheckBox { Text = "Can minify" };
            lblFenceSize = new Label { Text = "Size:" };
            numFenceWidth = new NumericUpDown { Minimum = 200, Maximum = 2000, Value = 300 };
            numFenceHeight = new NumericUpDown { Minimum = 200, Maximum = 2000, Value = 300 };
            numFenceTitleHeight = new NumericUpDown { Minimum = 16, Maximum = 100, Value = 35 };
            
            grpFenceColors = new GroupBox { Text = "Colors & Style" };
            btnFenceBackgroundColor = new Button { Text = "Background", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnFenceTitleBackgroundColor = new Button { Text = "Title Bg", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnFenceTextColor = new Button { Text = "Text", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            btnFenceBorderColor = new Button { Text = "Border", Size = new Size(80, 23), FlatStyle = FlatStyle.Flat };
            numFenceBackgroundTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numFenceTitleBackgroundTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 80, Size = new Size(50, 23) };
            numFenceTextTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numFenceBorderTransparency = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 100, Size = new Size(50, 23) };
            numFenceBorderWidth = new NumericUpDown { Minimum = 0, Maximum = 10, Value = 0 };
            numFenceCornerRadius = new NumericUpDown { Minimum = 0, Maximum = 20, Value = 0 };
            chkFenceShowShadow = new CheckBox { Text = "Show shadow" };
            cmbFenceIconSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFenceIconSize.Items.AddRange(new[] { "16", "24", "32", "48", "64" });
            numFenceItemSpacing = new NumericUpDown { Minimum = 5, Maximum = 50, Value = 15 };
            
            btnResetFenceToDefaults = new Button { Text = "Reset to Defaults", Size = new Size(120, 23) };
            btnSetFenceAsDefaults = new Button { 
                Text = "Set as Defaults", 
                Size = new Size(120, 23),
                UseVisualStyleBackColor = true
            };
            
            // Initialize tooltip
            toolTip = new ToolTip();
            toolTip.SetToolTip(btnResetFenceToDefaults, "Reset this fence to use the current default settings");
            toolTip.SetToolTip(btnSetFenceAsDefaults, "Use this fence's settings as the new defaults for future fences");
            
            // Button separator
            pnlButtonSeparator = new Panel {
                Height = 1,
                BackColor = SystemColors.ControlDark,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            // Dialog buttons
            btnOK = new Button { 
                Text = "OK", 
                Size = new Size(75, 28), 
                DialogResult = DialogResult.OK,
                UseVisualStyleBackColor = true,
                TabIndex = 1000
            };
            btnCancel = new Button { 
                Text = "Cancel", 
                Size = new Size(75, 28), 
                DialogResult = DialogResult.Cancel,
                UseVisualStyleBackColor = true,
                TabIndex = 1001
            };
            btnApply = new Button { 
                Text = "Apply", 
                Size = new Size(75, 28),
                UseVisualStyleBackColor = true,
                TabIndex = 999
            };
            
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void LayoutControls()
        {
            // Main tab control - leave proper space for buttons at bottom
            mainTabControl.Location = new Point(12, 12);
            mainTabControl.Size = new Size(this.ClientSize.Width - 24, this.ClientSize.Height - 70);
            
            // Add tabs
            mainTabControl.TabPages.AddRange(new[] { tabGlobal, tabDefaults, tabFences });
            
            // Layout Global Settings tab
            LayoutGlobalTab();
            
            // Layout Default Settings tab
            LayoutDefaultsTab();
            
            // Layout Fence Management tab
            LayoutFencesTab();
            
            // Button separator
            pnlButtonSeparator.Location = new Point(0, this.ClientSize.Height - 55);
            pnlButtonSeparator.Size = new Size(this.ClientSize.Width, 1);
            
            // Layout dialog buttons - position them at the bottom with proper spacing
            var buttonY = this.ClientSize.Height - 40;
            var buttonSpacing = 10;
            
            btnCancel.Location = new Point(this.ClientSize.Width - 85, buttonY);
            btnOK.Location = new Point(btnCancel.Left - btnOK.Width - buttonSpacing, buttonY);
            btnApply.Location = new Point(btnOK.Left - btnApply.Width - buttonSpacing, buttonY);
            
            // Add controls to form
            this.Controls.AddRange(new Control[] { mainTabControl, pnlButtonSeparator, btnApply, btnOK, btnCancel });
        }

        private void LayoutGlobalTab()
        {
            // Logging group
            grpLogging.Location = new Point(10, 10);
            grpLogging.Size = new Size(370, 80);
            
            var lblLogLevel = new Label { Text = "Log Level:", Location = new Point(10, 25), Size = new Size(90, 23) };
            cmbLogLevel.Location = new Point(110, 22);
            cmbLogLevel.Size = new Size(100, 23);
            chkEnableFileLogging.Location = new Point(230, 25);
            
            grpLogging.Controls.AddRange(new Control[] { lblLogLevel, cmbLogLevel, chkEnableFileLogging });
            
            // Hotkeys group
            grpHotkeys.Location = new Point(10, 100);
            grpHotkeys.Size = new Size(750, 280);
            
            var lblToggleTransparency = new Label { Text = "Toggle Transparency:", Location = new Point(10, 25), Size = new Size(130, 23) };
            txtToggleTransparencyShortcut.Location = new Point(150, 22);
            
            var lblToggleAutoHide = new Label { Text = "Toggle Auto-Hide:", Location = new Point(10, 55), Size = new Size(130, 23) };
            txtToggleAutoHideShortcut.Location = new Point(150, 52);
            
            var lblShowAllFences = new Label { Text = "Show All Fences:", Location = new Point(10, 85), Size = new Size(130, 23) };
            txtShowAllFencesShortcut.Location = new Point(150, 82);
            
            var lblCreateNewFence = new Label { Text = "Create New Fence:", Location = new Point(10, 115), Size = new Size(130, 23) };
            txtCreateNewFenceShortcut.Location = new Point(150, 112);
            
            var lblOpenSettings = new Label { Text = "Open Settings:", Location = new Point(300, 25), Size = new Size(130, 23) };
            txtOpenSettingsShortcut.Location = new Point(440, 22);
            
            var lblToggleLock = new Label { Text = "Toggle Lock:", Location = new Point(300, 55), Size = new Size(130, 23) };
            txtToggleLockShortcut.Location = new Point(440, 52);
            
            var lblMinimizeAll = new Label { Text = "Minimize All:", Location = new Point(300, 85), Size = new Size(130, 23) };
            txtMinimizeAllFencesShortcut.Location = new Point(440, 82);
            
            var lblRefresh = new Label { Text = "Refresh Fences:", Location = new Point(300, 115), Size = new Size(130, 23) };
            txtRefreshFencesShortcut.Location = new Point(440, 112);
            
            grpHotkeys.Controls.AddRange(new Control[] { 
                lblToggleTransparency, txtToggleTransparencyShortcut,
                lblToggleAutoHide, txtToggleAutoHideShortcut,
                lblShowAllFences, txtShowAllFencesShortcut,
                lblCreateNewFence, txtCreateNewFenceShortcut,
                lblOpenSettings, txtOpenSettingsShortcut,
                lblToggleLock, txtToggleLockShortcut,
                lblMinimizeAll, txtMinimizeAllFencesShortcut,
                lblRefresh, txtRefreshFencesShortcut
            });
            
            // Application group
            grpApplication.Location = new Point(10, 390);
            grpApplication.Size = new Size(370, 80);
            
            chkAutoSave.Location = new Point(10, 25);
            var lblAutoSaveInterval = new Label { Text = "Interval (s):", Location = new Point(130, 25), Size = new Size(80, 23) };
            numAutoSaveInterval.Location = new Point(220, 22);
            numAutoSaveInterval.Size = new Size(60, 23);
            
            chkShowTooltips.Location = new Point(10, 50);
            chkEnableAnimations.Location = new Point(130, 50);
            
            grpApplication.Controls.AddRange(new Control[] { 
                chkAutoSave, lblAutoSaveInterval, numAutoSaveInterval,
                chkShowTooltips, chkEnableAnimations
            });
            
            tabGlobal.Controls.AddRange(new Control[] { grpLogging, grpHotkeys, grpApplication });
        }

        private void LayoutDefaultsTab()
        {
            // Default size group
            grpDefaultSize.Location = new Point(10, 10);
            grpDefaultSize.Size = new Size(370, 100);
            
            var lblWidth = new Label { Text = "Width:", Location = new Point(10, 25), Size = new Size(60, 23) };
            numDefaultWidth.Location = new Point(80, 22);
            numDefaultWidth.Size = new Size(80, 23);
            
            var lblHeight = new Label { Text = "Height:", Location = new Point(180, 25), Size = new Size(60, 23) };
            numDefaultHeight.Location = new Point(250, 22);
            numDefaultHeight.Size = new Size(80, 23);
            
            var lblTitleHeight = new Label { Text = "Title Height:", Location = new Point(10, 55), Size = new Size(90, 23) };
            numDefaultTitleHeight.Location = new Point(110, 52);
            numDefaultTitleHeight.Size = new Size(80, 23);
            
            grpDefaultSize.Controls.AddRange(new Control[] { 
                lblWidth, numDefaultWidth, lblHeight, numDefaultHeight, 
                lblTitleHeight, numDefaultTitleHeight 
            });
            
            // Default appearance group
            grpDefaultAppearance.Location = new Point(10, 120);
            grpDefaultAppearance.Size = new Size(370, 100);
            
            var lblTransparency = new Label { Text = "Transparency:", Location = new Point(10, 25), Size = new Size(100, 23) };
            numDefaultTransparency.Location = new Point(120, 22);
            numDefaultTransparency.Size = new Size(60, 23);
            
            chkDefaultAutoHide.Location = new Point(10, 55);
            
            var lblAutoHideDelay = new Label { Text = "Delay (ms):", Location = new Point(140, 55), Size = new Size(80, 23) };
            numDefaultAutoHideDelay.Location = new Point(230, 52);
            numDefaultAutoHideDelay.Size = new Size(80, 23);
            
            grpDefaultAppearance.Controls.AddRange(new Control[] { 
                lblTransparency, numDefaultTransparency, 
                chkDefaultAutoHide, lblAutoHideDelay, numDefaultAutoHideDelay 
            });
            
            // Default colors group
            grpDefaultColors.Location = new Point(10, 230);
            grpDefaultColors.Size = new Size(950, 200);
            
            // Row 1: Color buttons
            btnDefaultBackgroundColor.Location = new Point(10, 25);
            btnDefaultTitleBackgroundColor.Location = new Point(100, 25);
            btnDefaultTextColor.Location = new Point(190, 25);
            btnDefaultBorderColor.Location = new Point(280, 25);
            
            // Row 2: Transparency labels and controls
            var lblBackgroundTransparency = new Label { Text = "Transparency:", Location = new Point(10, 55), Size = new Size(80, 15) };
            numDefaultBackgroundTransparency.Location = new Point(10, 70);
            
            var lblTitleBgTransparency = new Label { Text = "Transparency:", Location = new Point(100, 55), Size = new Size(80, 15) };
            numDefaultTitleBackgroundTransparency.Location = new Point(100, 70);
            
            var lblTextTransparency = new Label { Text = "Transparency:", Location = new Point(190, 55), Size = new Size(80, 15) };
            numDefaultTextTransparency.Location = new Point(190, 70);
            
            var lblBorderTransparency = new Label { Text = "Transparency:", Location = new Point(280, 55), Size = new Size(80, 15) };
            numDefaultBorderTransparency.Location = new Point(280, 70);
            
            // Row 3: Style controls
            var lblBorderWidth = new Label { Text = "Border Width:", Location = new Point(10, 105), Size = new Size(90, 23) };
            numDefaultBorderWidth.Location = new Point(110, 102);
            numDefaultBorderWidth.Size = new Size(60, 23);
            
            var lblCornerRadius = new Label { Text = "Corner Radius:", Location = new Point(200, 105), Size = new Size(90, 23) };
            numDefaultCornerRadius.Location = new Point(300, 102);
            numDefaultCornerRadius.Size = new Size(60, 23);
            
            chkDefaultShowShadow.Location = new Point(390, 105);
            
            var lblIconSize = new Label { Text = "Icon Size:", Location = new Point(10, 135), Size = new Size(70, 23) };
            cmbDefaultIconSize.Location = new Point(90, 132);
            cmbDefaultIconSize.Size = new Size(80, 23);
            
            var lblItemSpacing = new Label { Text = "Item Spacing:", Location = new Point(200, 135), Size = new Size(90, 23) };
            numDefaultItemSpacing.Location = new Point(300, 132);
            numDefaultItemSpacing.Size = new Size(60, 23);
            
            grpDefaultColors.Controls.AddRange(new Control[] { 
                btnDefaultBackgroundColor, btnDefaultTitleBackgroundColor, btnDefaultTextColor, btnDefaultBorderColor,
                lblBackgroundTransparency, numDefaultBackgroundTransparency,
                lblTitleBgTransparency, numDefaultTitleBackgroundTransparency,
                lblTextTransparency, numDefaultTextTransparency,
                lblBorderTransparency, numDefaultBorderTransparency,
                lblBorderWidth, numDefaultBorderWidth, lblCornerRadius, numDefaultCornerRadius, chkDefaultShowShadow,
                lblIconSize, cmbDefaultIconSize, lblItemSpacing, numDefaultItemSpacing
            });
            
            tabDefaults.Controls.AddRange(new Control[] { grpDefaultSize, grpDefaultAppearance, grpDefaultColors });
        }

        private void LayoutFencesTab()
        {
            // Fence list group
            grpFenceList.Location = new Point(10, 10);
            grpFenceList.Size = new Size(280, 380);
            
            lstFences.Location = new Point(10, 20);
            lstFences.Size = new Size(260, 270);
            
            btnRefreshFences.Location = new Point(10, 300);
            btnHighlightFence.Location = new Point(95, 300);
            btnShowAllFences.Location = new Point(180, 300);
            btnAddFence.Location = new Point(85, 330);
            
            grpFenceList.Controls.AddRange(new Control[] { lstFences, btnRefreshFences, btnHighlightFence, btnShowAllFences, btnAddFence });
            
            // Fence settings group
            grpFenceSettings.Location = new Point(300, 10);
            grpFenceSettings.Size = new Size(320, 350);
            
            lblFenceName.Location = new Point(10, 25);
            lblFenceName.Size = new Size(60, 23);
            txtFenceName.Location = new Point(80, 22);
            txtFenceName.Size = new Size(220, 23);
            
            lblFenceTransparency.Location = new Point(10, 55);
            lblFenceTransparency.Size = new Size(100, 23);
            numFenceTransparency.Location = new Point(120, 52);
            numFenceTransparency.Size = new Size(60, 23);
            
            chkFenceAutoHide.Location = new Point(10, 85);
            chkFenceAutoHide.Size = new Size(120, 23);
            var lblFenceAutoHideDelay = new Label { Text = "Delay (ms):", Location = new Point(10, 115), Size = new Size(80, 23) };
            numFenceAutoHideDelay.Location = new Point(100, 112);
            numFenceAutoHideDelay.Size = new Size(80, 23);
            
            chkFenceLocked.Location = new Point(10, 145);
            chkFenceCanMinify.Location = new Point(10, 175);
            
            lblFenceSize.Location = new Point(10, 210);
            lblFenceSize.Size = new Size(60, 23);
            var lblFenceWidth = new Label { Text = "Width:", Location = new Point(10, 235), Size = new Size(50, 23) };
            numFenceWidth.Location = new Point(70, 232);
            numFenceWidth.Size = new Size(80, 23);
            
            var lblFenceHeight = new Label { Text = "Height:", Location = new Point(165, 235), Size = new Size(50, 23) };
            numFenceHeight.Location = new Point(225, 232);
            numFenceHeight.Size = new Size(80, 23);
            
            var lblFenceTitleHeight = new Label { Text = "Title Height:", Location = new Point(10, 265), Size = new Size(90, 23) };
            numFenceTitleHeight.Location = new Point(110, 262);
            numFenceTitleHeight.Size = new Size(80, 23);
            
            btnResetFenceToDefaults.Location = new Point(10, 300);
            btnSetFenceAsDefaults.Location = new Point(140, 300);
            
            grpFenceSettings.Controls.AddRange(new Control[] { 
                lblFenceName, txtFenceName, lblFenceTransparency, numFenceTransparency,
                chkFenceAutoHide, lblFenceAutoHideDelay, numFenceAutoHideDelay, chkFenceLocked, chkFenceCanMinify,
                lblFenceSize, lblFenceWidth, numFenceWidth, lblFenceHeight, numFenceHeight,
                lblFenceTitleHeight, numFenceTitleHeight, btnResetFenceToDefaults, btnSetFenceAsDefaults
            });
            
            // Fence colors group
            grpFenceColors.Location = new Point(630, 10);
            grpFenceColors.Size = new Size(320, 450);
            
            // Row 1: Color buttons
            btnFenceBackgroundColor.Location = new Point(10, 25);
            btnFenceTitleBackgroundColor.Location = new Point(100, 25);
            btnFenceTextColor.Location = new Point(190, 25);
            btnFenceBorderColor.Location = new Point(10, 75);
            
            // Row 2: Transparency labels and controls
            var lblFenceBackgroundTransparency = new Label { Text = "Transparency:", Location = new Point(10, 50), Size = new Size(80, 15) };
            numFenceBackgroundTransparency.Location = new Point(10, 65);
            
            var lblFenceTitleBgTransparency = new Label { Text = "Transparency:", Location = new Point(100, 50), Size = new Size(80, 15) };
            numFenceTitleBackgroundTransparency.Location = new Point(100, 65);
            
            var lblFenceTextTransparency = new Label { Text = "Transparency:", Location = new Point(190, 50), Size = new Size(80, 15) };
            numFenceTextTransparency.Location = new Point(190, 65);
            
            var lblFenceBorderTransparency = new Label { Text = "Transparency:", Location = new Point(100, 100), Size = new Size(80, 15) };
            numFenceBorderTransparency.Location = new Point(100, 115);
            
            // Style controls
            var lblFenceBorderWidth = new Label { Text = "Border Width:", Location = new Point(10, 145), Size = new Size(90, 23) };
            numFenceBorderWidth.Location = new Point(110, 142);
            numFenceBorderWidth.Size = new Size(60, 23);
            
            var lblFenceCornerRadius = new Label { Text = "Corner Radius:", Location = new Point(10, 175), Size = new Size(90, 23) };
            numFenceCornerRadius.Location = new Point(110, 172);
            numFenceCornerRadius.Size = new Size(60, 23);
            
            chkFenceShowShadow.Location = new Point(10, 205);
            
            var lblFenceIconSize = new Label { Text = "Icon Size:", Location = new Point(10, 235), Size = new Size(70, 23) };
            cmbFenceIconSize.Location = new Point(90, 232);
            cmbFenceIconSize.Size = new Size(80, 23);
            
            var lblFenceItemSpacing = new Label { Text = "Item Spacing:", Location = new Point(10, 265), Size = new Size(90, 23) };
            numFenceItemSpacing.Location = new Point(110, 262);
            numFenceItemSpacing.Size = new Size(60, 23);
            
            grpFenceColors.Controls.AddRange(new Control[] { 
                btnFenceBackgroundColor, btnFenceTitleBackgroundColor, btnFenceTextColor, btnFenceBorderColor,
                lblFenceBackgroundTransparency, numFenceBackgroundTransparency,
                lblFenceTitleBgTransparency, numFenceTitleBackgroundTransparency,
                lblFenceTextTransparency, numFenceTextTransparency,
                lblFenceBorderTransparency, numFenceBorderTransparency,
                lblFenceBorderWidth, numFenceBorderWidth, lblFenceCornerRadius, numFenceCornerRadius, chkFenceShowShadow,
                lblFenceIconSize, cmbFenceIconSize, lblFenceItemSpacing, numFenceItemSpacing
            });
            
            tabFences.Controls.AddRange(new Control[] { grpFenceList, grpFenceSettings, grpFenceColors });
        }

        private void SetupEventHandlers()
        {
            lstFences.SelectedIndexChanged += LstFences_SelectedIndexChanged;
            btnRefreshFences.Click += BtnRefreshFences_Click;
            btnHighlightFence.Click += BtnHighlightFence_Click;
            btnShowAllFences.Click += BtnShowAllFences_Click;
            btnAddFence.Click += BtnAddFence_Click;
            btnResetFenceToDefaults.Click += BtnResetFenceToDefaults_Click;
            btnSetFenceAsDefaults.Click += BtnSetFenceAsDefaults_Click;
            btnApply.Click += BtnApply_Click;
            btnOK.Click += BtnOK_Click;
            
            // Color button event handlers
            btnDefaultBackgroundColor.Click += (s, e) => ShowColorPicker(btnDefaultBackgroundColor, "defaultBackground");
            btnDefaultTitleBackgroundColor.Click += (s, e) => ShowColorPicker(btnDefaultTitleBackgroundColor, "defaultTitleBackground");
            btnDefaultTextColor.Click += (s, e) => ShowColorPicker(btnDefaultTextColor, "defaultText");
            btnDefaultBorderColor.Click += (s, e) => ShowColorPicker(btnDefaultBorderColor, "defaultBorder");
            
            btnFenceBackgroundColor.Click += (s, e) => ShowColorPicker(btnFenceBackgroundColor, "fenceBackground");
            btnFenceTitleBackgroundColor.Click += (s, e) => ShowColorPicker(btnFenceTitleBackgroundColor, "fenceTitleBackground");
            btnFenceTextColor.Click += (s, e) => ShowColorPicker(btnFenceTextColor, "fenceText");
            btnFenceBorderColor.Click += (s, e) => ShowColorPicker(btnFenceBorderColor, "fenceBorder");
            
            // Enable/disable auto-hide delay based on checkbox
            chkDefaultAutoHide.CheckedChanged += (s, e) => numDefaultAutoHideDelay.Enabled = chkDefaultAutoHide.Checked;
            chkFenceAutoHide.CheckedChanged += (s, e) => numFenceAutoHideDelay.Enabled = chkFenceAutoHide.Checked;
            
            // Auto-apply fence settings when controls change
            txtFenceName.TextChanged += OnFenceSettingChanged;
            numFenceTransparency.ValueChanged += OnFenceSettingChanged;
            chkFenceAutoHide.CheckedChanged += OnFenceSettingChanged;
            numFenceAutoHideDelay.ValueChanged += OnFenceSettingChanged;
            chkFenceLocked.CheckedChanged += OnFenceSettingChanged;
            chkFenceCanMinify.CheckedChanged += OnFenceSettingChanged;
            numFenceWidth.ValueChanged += OnFenceSettingChanged;
            numFenceHeight.ValueChanged += OnFenceSettingChanged;
            numFenceTitleHeight.ValueChanged += OnFenceSettingChanged;
            numFenceBorderWidth.ValueChanged += OnFenceSettingChanged;
            numFenceCornerRadius.ValueChanged += OnFenceSettingChanged;
            chkFenceShowShadow.CheckedChanged += OnFenceSettingChanged;
            cmbFenceIconSize.SelectedIndexChanged += OnFenceSettingChanged;
            numFenceItemSpacing.ValueChanged += OnFenceSettingChanged;
            numFenceBackgroundTransparency.ValueChanged += OnFenceSettingChanged;
            numFenceTitleBackgroundTransparency.ValueChanged += OnFenceSettingChanged;
            numFenceTextTransparency.ValueChanged += OnFenceSettingChanged;
            numFenceBorderTransparency.ValueChanged += OnFenceSettingChanged;
        }

        private void LoadSettings()
        {
            try
            {
                logger.Debug("Loading settings into global settings form", "GlobalSettingsForm");
                var settings = AppSettings.Instance;
                
                // Global settings
                cmbLogLevel.SelectedItem = settings.LogLevel;
                chkEnableFileLogging.Checked = settings.EnableFileLogging;
                chkAutoSave.Checked = settings.AutoSave;
                numAutoSaveInterval.Value = settings.AutoSaveInterval;
                chkShowTooltips.Checked = settings.ShowTooltips;
                chkEnableAnimations.Checked = settings.EnableAnimations;
                
                // Shortcuts
                txtToggleTransparencyShortcut.Text = settings.ToggleTransparencyShortcut;
                txtToggleAutoHideShortcut.Text = settings.ToggleAutoHideShortcut;
                txtShowAllFencesShortcut.Text = settings.ShowAllFencesShortcut;
                txtCreateNewFenceShortcut.Text = settings.CreateNewFenceShortcut;
                txtOpenSettingsShortcut.Text = settings.OpenSettingsShortcut;
                txtToggleLockShortcut.Text = settings.ToggleLockShortcut;
                txtMinimizeAllFencesShortcut.Text = settings.MinimizeAllFencesShortcut;
                txtRefreshFencesShortcut.Text = settings.RefreshFencesShortcut;
                
                // Default settings
                numDefaultWidth.Value = settings.DefaultFenceWidth;
                numDefaultHeight.Value = settings.DefaultFenceHeight;
                numDefaultTitleHeight.Value = settings.DefaultTitleHeight;
                numDefaultTransparency.Value = settings.DefaultTransparency;
                chkDefaultAutoHide.Checked = settings.DefaultAutoHide;
                numDefaultAutoHideDelay.Value = settings.DefaultAutoHideDelay;
                numDefaultAutoHideDelay.Enabled = settings.DefaultAutoHide;
                
                // Default colors
                UpdateColorButton(btnDefaultBackgroundColor, Color.FromArgb(settings.DefaultBackgroundColor));
                UpdateColorButton(btnDefaultTitleBackgroundColor, Color.FromArgb(settings.DefaultTitleBackgroundColor));
                UpdateColorButton(btnDefaultTextColor, Color.FromArgb(settings.DefaultTextColor));
                UpdateColorButton(btnDefaultBorderColor, Color.FromArgb(settings.DefaultBorderColor));
                numDefaultBackgroundTransparency.Value = settings.DefaultBackgroundTransparency;
                numDefaultTitleBackgroundTransparency.Value = settings.DefaultTitleBackgroundTransparency;
                numDefaultTextTransparency.Value = settings.DefaultTextTransparency;
                numDefaultBorderTransparency.Value = settings.DefaultBorderTransparency;
                numDefaultBorderWidth.Value = settings.DefaultBorderWidth;
                numDefaultCornerRadius.Value = settings.DefaultCornerRadius;
                chkDefaultShowShadow.Checked = settings.DefaultShowShadow;
                cmbDefaultIconSize.SelectedItem = settings.DefaultIconSize.ToString();
                numDefaultItemSpacing.Value = settings.DefaultItemSpacing;
                
                logger.Info("Settings loaded successfully", "GlobalSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load settings", "GlobalSettingsForm", ex);
            }
        }

        private void LoadFences()
        {
            try
            {
                logger.Debug("Loading fences into fence management list", "GlobalSettingsForm");
                
                // Get fresh fence data from the manager
                fenceInfos = FenceManager.Instance.GetAllFenceInfos();
                
                // Remember the currently selected fence ID if any
                Guid? selectedId = selectedFenceInfo?.Id;
                
                lstFences.Items.Clear();
                int newSelectedIndex = -1;
                
                for (int i = 0; i < fenceInfos.Count; i++)
                {
                    var fence = fenceInfos[i];
                    lstFences.Items.Add($"{fence.Name} (ID: {fence.Id.ToString().Substring(0, 8)}...)");
                    
                    // If this was the previously selected fence, remember its new index
                    if (selectedId.HasValue && fence.Id == selectedId.Value)
                    {
                        newSelectedIndex = i;
                    }
                }
                
                // Restore selection if the fence still exists
                if (newSelectedIndex >= 0)
                {
                    lstFences.SelectedIndex = newSelectedIndex;
                }
                else
                {
                    selectedFenceInfo = null;
                    grpFenceSettings.Enabled = false;
                }
                
                logger.Info($"Loaded {fenceInfos.Count} fences", "GlobalSettingsForm");
                
                // Clear fence settings if no fences
                if (fenceInfos.Count == 0)
                {
                    grpFenceSettings.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load fences", "GlobalSettingsForm", ex);
            }
        }

        private void LstFences_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lstFences.SelectedIndex >= 0 && lstFences.SelectedIndex < fenceInfos.Count)
                {
                    selectedFenceInfo = fenceInfos[lstFences.SelectedIndex];
                    LoadFenceSettings();
                    grpFenceSettings.Enabled = true;
                    
                    logger.Debug($"Selected fence '{selectedFenceInfo.Name}'", "GlobalSettingsForm");
                }
                else
                {
                    selectedFenceInfo = null;
                    grpFenceSettings.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to select fence", "GlobalSettingsForm", ex);
            }
        }

        private void LoadFenceSettings()
        {
            if (selectedFenceInfo == null) return;
            
            try
            {
                isUpdatingControls = true;
                
                txtFenceName.Text = selectedFenceInfo.Name;
                numFenceTransparency.Value = selectedFenceInfo.Transparency;
                chkFenceAutoHide.Checked = selectedFenceInfo.AutoHide;
                numFenceAutoHideDelay.Value = selectedFenceInfo.AutoHideDelay;
                numFenceAutoHideDelay.Enabled = selectedFenceInfo.AutoHide;
                chkFenceLocked.Checked = selectedFenceInfo.Locked;
                chkFenceCanMinify.Checked = selectedFenceInfo.CanMinify;
                numFenceWidth.Value = selectedFenceInfo.Width;
                numFenceHeight.Value = selectedFenceInfo.Height;
                numFenceTitleHeight.Value = selectedFenceInfo.TitleHeight;
                
                // Color and style settings
                UpdateColorButton(btnFenceBackgroundColor, Color.FromArgb(selectedFenceInfo.BackgroundColor));
                UpdateColorButton(btnFenceTitleBackgroundColor, Color.FromArgb(selectedFenceInfo.TitleBackgroundColor));
                UpdateColorButton(btnFenceTextColor, Color.FromArgb(selectedFenceInfo.TextColor));
                UpdateColorButton(btnFenceBorderColor, Color.FromArgb(selectedFenceInfo.BorderColor));
                numFenceBackgroundTransparency.Value = selectedFenceInfo.BackgroundTransparency;
                numFenceTitleBackgroundTransparency.Value = selectedFenceInfo.TitleBackgroundTransparency;
                numFenceTextTransparency.Value = selectedFenceInfo.TextTransparency;
                numFenceBorderTransparency.Value = selectedFenceInfo.BorderTransparency;
                numFenceBorderWidth.Value = selectedFenceInfo.BorderWidth;
                numFenceCornerRadius.Value = selectedFenceInfo.CornerRadius;
                chkFenceShowShadow.Checked = selectedFenceInfo.ShowShadow;
                cmbFenceIconSize.SelectedItem = selectedFenceInfo.IconSize.ToString();
                numFenceItemSpacing.Value = selectedFenceInfo.ItemSpacing;
                
                isUpdatingControls = false;
                
                logger.Debug($"Loaded settings for fence '{selectedFenceInfo.Name}'", "GlobalSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load fence settings for '{selectedFenceInfo?.Name}'", "GlobalSettingsForm", ex);
                isUpdatingControls = false;
            }
        }

        private void ShowColorPicker(Button colorButton, string colorType)
        {
            try
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = colorButton.BackColor;
                    colorDialog.FullOpen = true;
                    
                    if (colorDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateColorButton(colorButton, colorDialog.Color);
                        
                        // Apply the color change based on type
                        if (colorType.StartsWith("fence") && selectedFenceInfo != null)
                        {
                            switch (colorType)
                            {
                                case "fenceBackground":
                                    selectedFenceInfo.BackgroundColor = colorDialog.Color.ToArgb();
                                    break;
                                case "fenceTitleBackground":
                                    selectedFenceInfo.TitleBackgroundColor = colorDialog.Color.ToArgb();
                                    break;
                                case "fenceText":
                                    selectedFenceInfo.TextColor = colorDialog.Color.ToArgb();
                                    break;
                                case "fenceBorder":
                                    selectedFenceInfo.BorderColor = colorDialog.Color.ToArgb();
                                    break;
                            }
                            OnFenceSettingChanged(colorButton, EventArgs.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to show color picker for {colorType}", "GlobalSettingsForm", ex);
            }
        }

        private void UpdateColorButton(Button button, Color color)
        {
            button.BackColor = color;
            button.ForeColor = GetContrastColor(color);
        }

        private Color GetContrastColor(Color color)
        {
            // Calculate luminance to determine if we should use black or white text
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance > 0.5 ? Color.Black : Color.White;
        }

        private void ApplyFenceSettings()
        {
            if (selectedFenceInfo == null) return;
            
            // Store the old name for logging
            var oldName = selectedFenceInfo.Name;
            
            // Update the fence info object
            selectedFenceInfo.Name = txtFenceName.Text;
            selectedFenceInfo.Transparency = (int)numFenceTransparency.Value;
            selectedFenceInfo.AutoHide = chkFenceAutoHide.Checked;
            selectedFenceInfo.AutoHideDelay = (int)numFenceAutoHideDelay.Value;
            selectedFenceInfo.Locked = chkFenceLocked.Checked;
            selectedFenceInfo.CanMinify = chkFenceCanMinify.Checked;
            selectedFenceInfo.Width = (int)numFenceWidth.Value;
            selectedFenceInfo.Height = (int)numFenceHeight.Value;
            selectedFenceInfo.TitleHeight = (int)numFenceTitleHeight.Value;
            
            // Color and style settings
            selectedFenceInfo.BackgroundColor = btnFenceBackgroundColor.BackColor.ToArgb();
            selectedFenceInfo.TitleBackgroundColor = btnFenceTitleBackgroundColor.BackColor.ToArgb();
            selectedFenceInfo.TextColor = btnFenceTextColor.BackColor.ToArgb();
            selectedFenceInfo.BorderColor = btnFenceBorderColor.BackColor.ToArgb();
            selectedFenceInfo.BackgroundTransparency = (int)numFenceBackgroundTransparency.Value;
            selectedFenceInfo.TitleBackgroundTransparency = (int)numFenceTitleBackgroundTransparency.Value;
            selectedFenceInfo.TextTransparency = (int)numFenceTextTransparency.Value;
            selectedFenceInfo.BorderTransparency = (int)numFenceBorderTransparency.Value;
            selectedFenceInfo.BorderWidth = (int)numFenceBorderWidth.Value;
            selectedFenceInfo.CornerRadius = (int)numFenceCornerRadius.Value;
            selectedFenceInfo.ShowShadow = chkFenceShowShadow.Checked;
            if (int.TryParse(cmbFenceIconSize.SelectedItem?.ToString(), out int iconSize))
                selectedFenceInfo.IconSize = iconSize;
            selectedFenceInfo.ItemSpacing = (int)numFenceItemSpacing.Value;
            
            // Save to storage
            FenceManager.Instance.UpdateFence(selectedFenceInfo);
            
            // Apply settings to the actual fence window
            FenceManager.Instance.ApplySettingsToFence(selectedFenceInfo);
            
            // Update the list display with the new name
            if (lstFences.SelectedIndex >= 0)
            {
                lstFences.Items[lstFences.SelectedIndex] = $"{selectedFenceInfo.Name} (ID: {selectedFenceInfo.Id.ToString().Substring(0, 8)}...)";
            }
            
            // Update the fenceInfos list to keep it in sync
            var index = fenceInfos.FindIndex(f => f.Id == selectedFenceInfo.Id);
            if (index >= 0)
            {
                fenceInfos[index] = selectedFenceInfo;
            }
            
            logger.Info($"Applied settings to fence '{oldName}' -> '{selectedFenceInfo.Name}'", "GlobalSettingsForm");
        }

        private void ApplyGlobalSettings()
        {
            try
            {
                logger.Debug("Applying global settings", "GlobalSettingsForm");
                var settings = AppSettings.Instance;
                
                // Global settings
                settings.LogLevel = cmbLogLevel.SelectedItem?.ToString() ?? "Info";
                settings.EnableFileLogging = chkEnableFileLogging.Checked;
                settings.AutoSave = chkAutoSave.Checked;
                settings.AutoSaveInterval = (int)numAutoSaveInterval.Value;
                settings.ShowTooltips = chkShowTooltips.Checked;
                settings.EnableAnimations = chkEnableAnimations.Checked;
                
                // Shortcuts
                settings.ToggleTransparencyShortcut = txtToggleTransparencyShortcut.Text;
                settings.ToggleAutoHideShortcut = txtToggleAutoHideShortcut.Text;
                settings.ShowAllFencesShortcut = txtShowAllFencesShortcut.Text;
                settings.CreateNewFenceShortcut = txtCreateNewFenceShortcut.Text;
                settings.OpenSettingsShortcut = txtOpenSettingsShortcut.Text;
                settings.ToggleLockShortcut = txtToggleLockShortcut.Text;
                settings.MinimizeAllFencesShortcut = txtMinimizeAllFencesShortcut.Text;
                settings.RefreshFencesShortcut = txtRefreshFencesShortcut.Text;
                
                // Default settings
                settings.DefaultFenceWidth = (int)numDefaultWidth.Value;
                settings.DefaultFenceHeight = (int)numDefaultHeight.Value;
                settings.DefaultTitleHeight = (int)numDefaultTitleHeight.Value;
                settings.DefaultTransparency = (int)numDefaultTransparency.Value;
                settings.DefaultAutoHide = chkDefaultAutoHide.Checked;
                settings.DefaultAutoHideDelay = (int)numDefaultAutoHideDelay.Value;
                
                // Default colors
                settings.DefaultBackgroundColor = btnDefaultBackgroundColor.BackColor.ToArgb();
                settings.DefaultTitleBackgroundColor = btnDefaultTitleBackgroundColor.BackColor.ToArgb();
                settings.DefaultTextColor = btnDefaultTextColor.BackColor.ToArgb();
                settings.DefaultBorderColor = btnDefaultBorderColor.BackColor.ToArgb();
                settings.DefaultBackgroundTransparency = (int)numDefaultBackgroundTransparency.Value;
                settings.DefaultTitleBackgroundTransparency = (int)numDefaultTitleBackgroundTransparency.Value;
                settings.DefaultTextTransparency = (int)numDefaultTextTransparency.Value;
                settings.DefaultBorderTransparency = (int)numDefaultBorderTransparency.Value;
                settings.DefaultBorderWidth = (int)numDefaultBorderWidth.Value;
                settings.DefaultCornerRadius = (int)numDefaultCornerRadius.Value;
                settings.DefaultShowShadow = chkDefaultShowShadow.Checked;
                if (int.TryParse(cmbDefaultIconSize.SelectedItem?.ToString(), out int defaultIconSize))
                    settings.DefaultIconSize = defaultIconSize;
                settings.DefaultItemSpacing = (int)numDefaultItemSpacing.Value;
                
                settings.SaveSettings();
                
                logger.Info("Global settings applied successfully", "GlobalSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply global settings", "GlobalSettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnResetFenceToDefaults_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                try
                {
                    var settings = AppSettings.Instance;
                    
                    selectedFenceInfo.Transparency = settings.DefaultTransparency;
                    selectedFenceInfo.AutoHide = settings.DefaultAutoHide;
                    selectedFenceInfo.AutoHideDelay = settings.DefaultAutoHideDelay;
                    selectedFenceInfo.Width = settings.DefaultFenceWidth;
                    selectedFenceInfo.Height = settings.DefaultFenceHeight;
                    selectedFenceInfo.TitleHeight = settings.DefaultTitleHeight;
                    selectedFenceInfo.Locked = false;
                    selectedFenceInfo.CanMinify = true;
                    
                    // Reset color and style settings to defaults
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
                    
                    logger.Info($"Reset fence '{selectedFenceInfo.Name}' to defaults", "GlobalSettingsForm");
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to reset fence to defaults", "GlobalSettingsForm", ex);
                    MessageBox.Show($"Failed to reset to defaults: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSetFenceAsDefaults_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                try
                {
                    var result = MessageBox.Show(
                        $"Set fence '{selectedFenceInfo.Name}' settings as new defaults for all future fences?\n\nThis will update all default settings in the Default Settings tab.",
                        "Set as Defaults",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        var settings = AppSettings.Instance;
                        
                        // Update default settings with current fence settings
                        settings.DefaultTransparency = selectedFenceInfo.Transparency;
                        settings.DefaultAutoHide = selectedFenceInfo.AutoHide;
                        settings.DefaultAutoHideDelay = selectedFenceInfo.AutoHideDelay;
                        settings.DefaultFenceWidth = selectedFenceInfo.Width;
                        settings.DefaultFenceHeight = selectedFenceInfo.Height;
                        settings.DefaultTitleHeight = selectedFenceInfo.TitleHeight;
                        
                        // Color and style settings
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
                        
                        // Save the settings
                        settings.SaveSettings();
                        
                        // Reload the settings in the UI to reflect the changes
                        LoadSettings();
                        
                        MessageBox.Show($"Fence '{selectedFenceInfo.Name}' settings have been set as defaults!", 
                            "Defaults Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        logger.Info($"Set fence '{selectedFenceInfo.Name}' settings as defaults", "GlobalSettingsForm");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to set fence '{selectedFenceInfo?.Name}' as defaults", "GlobalSettingsForm", ex);
                    MessageBox.Show($"Failed to set as defaults: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                toolTip?.Dispose();
                logger?.Debug("Disposing global settings form", "GlobalSettingsForm");
            }
            base.Dispose(disposing);
        }

        private void OnFenceSettingChanged(object sender, EventArgs e)
        {
            // Auto-apply fence settings when they change (with a small delay to avoid too many updates)
            if (selectedFenceInfo != null && !isUpdatingControls)
            {
                try
                {
                    ApplyFenceSettings();
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to auto-apply fence settings", "GlobalSettingsForm", ex);
                }
            }
        }
        
        private void BtnRefreshFences_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Debug("Manual refresh of fence list requested", "GlobalSettingsForm");
                LoadFences();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to refresh fence list", "GlobalSettingsForm", ex);
                MessageBox.Show($"Failed to refresh fence list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHighlightFence_Click(object sender, EventArgs e)
        {
            if (selectedFenceInfo != null)
            {
                try
                {
                    logger.Debug($"Highlighting fence '{selectedFenceInfo.Name}'", "GlobalSettingsForm");
                    FenceManager.Instance.HighlightFence(selectedFenceInfo.Id);
                    
                    // Also bring the settings window to front briefly to show the action worked
                    this.TopMost = true;
                    this.TopMost = false;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to highlight fence '{selectedFenceInfo.Name}'", "GlobalSettingsForm", ex);
                    MessageBox.Show($"Failed to highlight fence: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a fence to highlight.", "No Fence Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnShowAllFences_Click(object sender, EventArgs e)
        {
            try
            {
                logger.Debug("Showing all fences from settings", "GlobalSettingsForm");
                FenceManager.Instance.ShowAllFences();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to show all fences", "GlobalSettingsForm", ex);
            }
        }

        private void BtnAddFence_Click(object sender, EventArgs e)
        {
            try
            {
                // Prompt for fence name
                var dialog = new EditDialog("New Fence");
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var fenceName = dialog.NewName;
                    if (!string.IsNullOrWhiteSpace(fenceName))
                    {
                        logger.Info($"Creating new fence '{fenceName}' from settings", "GlobalSettingsForm");
                        FenceManager.Instance.CreateFence(fenceName);
                        
                        // Refresh the fence list
                        LoadFences();
                        
                        // Select the newly created fence if possible
                        var newFence = fenceInfos.FirstOrDefault(f => f.Name == fenceName);
                        if (newFence != null)
                        {
                            var index = fenceInfos.IndexOf(newFence);
                            if (index >= 0)
                            {
                                lstFences.SelectedIndex = index;
                                LoadFenceSettings();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to add new fence", "GlobalSettingsForm", ex);
                MessageBox.Show($"Failed to add new fence: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyGlobalSettings();
                
                // If a fence is selected, also apply its settings
                if (selectedFenceInfo != null)
                {
                    ApplyFenceSettings();
                }
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply settings on OK", "GlobalSettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                ApplyGlobalSettings();
                
                // Also apply any pending fence settings
                if (selectedFenceInfo != null)
                {
                    ApplyFenceSettings();
                }
                
                MessageBox.Show("Settings applied successfully!", "Settings Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply settings", "GlobalSettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}