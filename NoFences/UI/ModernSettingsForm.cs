using DarkUI.Controls;
using DarkUI.Forms;
using NoFences.Model;
using NoFences.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NoFences.UI
{
    public partial class ModernSettingsForm : DarkForm
    {
        private readonly Logger logger;
        private List<FenceInfo> fenceInfos;
        private FenceInfo selectedFenceInfo;
        private bool isUpdatingControls = false;

        // Main layout controls
        private DarkSectionPanel mainContainer;
        private TabControl mainTabControl;
        private TabPage globalTab;
        private TabPage defaultsTab;
        private TabPage fencesTab;

        // Global settings
        private Dictionary<string, Control> globalSettings = new Dictionary<string, Control>();
        
        // Default settings
        private Dictionary<string, Control> defaultSettings = new Dictionary<string, Control>();
        
        // Fence management
        private ListView fenceListView;
        private DarkGroupBox fenceSettingsGroup;
        private Dictionary<string, Control> fenceSettings = new Dictionary<string, Control>();
        
        // Buttons
        private DarkButton btnOK;
        private DarkButton btnCancel;
        private DarkButton btnApply;

        public ModernSettingsForm()
        {
            logger = Logger.Instance;
            logger.Debug("Creating modern settings form", "ModernSettingsForm");
            
            InitializeComponent();
            CreateDynamicControls();
            LoadSettings();
            LoadFences();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form setup with proper form border style for dragging
            this.Text = "BetterNoFences Settings";
            this.Size = new Size(1200, 800);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(1000, 600);
            this.ControlBox = true; // Ensure control box is visible

            // Create main container
            mainContainer = new DarkSectionPanel
            {
                Dock = DockStyle.Fill,
                SectionHeader = null
            };

            // Create tab control with dark styling
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220)
            };

            // Create tabs with dark styling
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

            // Create button panel
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
            
            // Add to main container
            mainContainer.Controls.Add(mainTabControl);
            this.Controls.Add(mainContainer);
            this.Controls.Add(buttonPanel);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Event handlers
            btnOK.Click += BtnOK_Click;
            btnApply.Click += BtnApply_Click;

            this.ResumeLayout(false);
        }

        private void CreateDynamicControls()
        {
            CreateGlobalSettingsTab();
            CreateDefaultSettingsTab();
            CreateFenceManagementTab();
        }

        private void CreateGlobalSettingsTab()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            var contentPanel = new Panel
            {
                AutoSize = true,
                Width = scrollPanel.Width - 25,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            int yPos = 10;

            // Application Settings Group
            var appGroup = CreateGroupBox("Application Settings", ref yPos, contentPanel.Width - 20);
            
            AddBooleanSetting(appGroup, "AutoSave", "Auto Save", "Automatically save changes", ref yPos);
            AddNumericSetting(appGroup, "AutoSaveInterval", "Auto Save Interval (seconds)", 5, 300, ref yPos);
            AddBooleanSetting(appGroup, "ShowTooltips", "Show Tooltips", "Display helpful tooltips", ref yPos);
            AddBooleanSetting(appGroup, "EnableAnimations", "Enable Animations", "Enable UI animations", ref yPos);
            
            FinalizeGroupBox(appGroup, yPos);
            contentPanel.Controls.Add(appGroup);
            yPos += appGroup.Height + 20;

            // Logging Settings Group
            var logGroup = CreateGroupBox("Logging Settings", ref yPos, contentPanel.Width - 20);
            
            AddComboBoxSetting(logGroup, "LogLevel", "Log Level", new[] { "Debug", "Info", "Warning", "Error", "Critical" }, ref yPos);
            AddBooleanSetting(logGroup, "EnableFileLogging", "Enable File Logging", "Write logs to file", ref yPos);
            
            FinalizeGroupBox(logGroup, yPos);
            contentPanel.Controls.Add(logGroup);
            yPos += logGroup.Height + 20;

            // Global Hotkeys Group
            var hotkeyGroup = CreateGroupBox("Global Hotkeys", ref yPos, contentPanel.Width - 20);
            
            AddHotkeySetting(hotkeyGroup, "ToggleTransparencyShortcut", "Toggle Transparency", ref yPos);
            AddHotkeySetting(hotkeyGroup, "ToggleAutoHideShortcut", "Toggle Auto-Hide", ref yPos);
            AddHotkeySetting(hotkeyGroup, "ShowAllFencesShortcut", "Show All Fences", ref yPos);
            AddHotkeySetting(hotkeyGroup, "CreateNewFenceShortcut", "Create New Fence", ref yPos);
            AddHotkeySetting(hotkeyGroup, "OpenSettingsShortcut", "Open Settings", ref yPos);
            AddHotkeySetting(hotkeyGroup, "ToggleLockShortcut", "Toggle Lock", ref yPos);
            AddHotkeySetting(hotkeyGroup, "MinimizeAllFencesShortcut", "Minimize All Fences", ref yPos);
            AddHotkeySetting(hotkeyGroup, "RefreshFencesShortcut", "Refresh Fences", ref yPos);
            
            FinalizeGroupBox(hotkeyGroup, yPos);
            contentPanel.Controls.Add(hotkeyGroup);

            contentPanel.Height = yPos + hotkeyGroup.Height + 20;
            scrollPanel.Controls.Add(contentPanel);
            globalTab.Controls.Add(scrollPanel);
        }

        private void CreateDefaultSettingsTab()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            var contentPanel = new Panel
            {
                AutoSize = true,
                Width = scrollPanel.Width - 25,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            int yPos = 10;

            // Size Settings Group
            var sizeGroup = CreateGroupBox("Default Size Settings", ref yPos, contentPanel.Width - 20);
            
            AddNumericSetting(sizeGroup, "DefaultFenceWidth", "Width", 200, 2000, ref yPos, defaultSettings);
            AddNumericSetting(sizeGroup, "DefaultFenceHeight", "Height", 200, 2000, ref yPos, defaultSettings);
            AddNumericSetting(sizeGroup, "DefaultTitleHeight", "Title Height", 16, 100, ref yPos, defaultSettings);
            
            FinalizeGroupBox(sizeGroup, yPos);
            contentPanel.Controls.Add(sizeGroup);
            yPos += sizeGroup.Height + 20;

            // Appearance Settings Group
            var appearanceGroup = CreateGroupBox("Default Appearance Settings", ref yPos, contentPanel.Width - 20);
            
            AddNumericSetting(appearanceGroup, "DefaultTransparency", "Transparency (%)", 25, 100, ref yPos, defaultSettings);
            AddBooleanSetting(appearanceGroup, "DefaultAutoHide", "Auto Hide", "Hide when not in use", ref yPos, defaultSettings);
            AddNumericSetting(appearanceGroup, "DefaultAutoHideDelay", "Auto Hide Delay (ms)", 500, 10000, ref yPos, defaultSettings);
            
            FinalizeGroupBox(appearanceGroup, yPos);
            contentPanel.Controls.Add(appearanceGroup);
            yPos += appearanceGroup.Height + 20;

            // Color Settings Group
            var colorGroup = CreateGroupBox("Default Color Settings", ref yPos, contentPanel.Width - 20);
            
            AddColorSetting(colorGroup, "DefaultBackgroundColor", "Background Color", ref yPos, defaultSettings);
            AddNumericSetting(colorGroup, "DefaultBackgroundTransparency", "Background Transparency (%)", 0, 100, ref yPos, defaultSettings);
            AddColorSetting(colorGroup, "DefaultTitleBackgroundColor", "Title Background Color", ref yPos, defaultSettings);
            AddNumericSetting(colorGroup, "DefaultTitleBackgroundTransparency", "Title Background Transparency (%)", 0, 100, ref yPos, defaultSettings);
            AddColorSetting(colorGroup, "DefaultTextColor", "Text Color", ref yPos, defaultSettings);
            AddNumericSetting(colorGroup, "DefaultTextTransparency", "Text Transparency (%)", 0, 100, ref yPos, defaultSettings);
            AddColorSetting(colorGroup, "DefaultBorderColor", "Border Color", ref yPos, defaultSettings);
            AddNumericSetting(colorGroup, "DefaultBorderTransparency", "Border Transparency (%)", 0, 100, ref yPos, defaultSettings);
            
            FinalizeGroupBox(colorGroup, yPos);
            contentPanel.Controls.Add(colorGroup);
            yPos += colorGroup.Height + 20;

            // Style Settings Group
            var styleGroup = CreateGroupBox("Default Style Settings", ref yPos, contentPanel.Width - 20);
            
            AddNumericSetting(styleGroup, "DefaultBorderWidth", "Border Width", 0, 10, ref yPos, defaultSettings);
            AddNumericSetting(styleGroup, "DefaultCornerRadius", "Corner Radius", 0, 20, ref yPos, defaultSettings);
            AddBooleanSetting(styleGroup, "DefaultShowShadow", "Show Shadow", "Display drop shadow", ref yPos, defaultSettings);
            AddComboBoxSetting(styleGroup, "DefaultIconSize", "Icon Size", new[] { "16", "24", "32", "48", "64" }, ref yPos, defaultSettings);
            AddNumericSetting(styleGroup, "DefaultItemSpacing", "Item Spacing", 5, 50, ref yPos, defaultSettings);
            
            FinalizeGroupBox(styleGroup, yPos);
            contentPanel.Controls.Add(styleGroup);

            contentPanel.Height = yPos + styleGroup.Height + 20;
            scrollPanel.Controls.Add(contentPanel);
            defaultsTab.Controls.Add(scrollPanel);
        }

        private void CreateFenceManagementTab()
        {
            // Create SplitContainer without setting SplitterDistance immediately
            var mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(60, 63, 65)
            };

            // Style the splitter panels
            mainSplitter.Panel1.BackColor = Color.FromArgb(60, 63, 65);
            mainSplitter.Panel2.BackColor = Color.FromArgb(60, 63, 65);

            // Defer applying min sizes and splitter distance until the form is loaded
            this.Load += (s, e) =>
            {
                try
                {
                    // Apply conservative minimums
                    mainSplitter.Panel1MinSize = 150;
                    mainSplitter.Panel2MinSize = 300;

                    int availableWidth = Math.Max(0, mainSplitter.ClientSize.Width);
                    int minRequiredWidth = mainSplitter.Panel1MinSize + mainSplitter.Panel2MinSize;

                    if (availableWidth >= minRequiredWidth)
                    {
                        int desiredLeft = Math.Min(250, availableWidth / 3);
                        int maxLeft = availableWidth - mainSplitter.Panel2MinSize;
                        int safeLeft = Math.Max(mainSplitter.Panel1MinSize, Math.Min(desiredLeft, maxLeft));

                        // Only set if within valid bounds
                        if (safeLeft >= mainSplitter.Panel1MinSize && safeLeft <= availableWidth - mainSplitter.Panel2MinSize)
                        {
                            mainSplitter.SplitterDistance = safeLeft;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.Warning($"Could not set SplitContainer sizes safely: {ex.Message}", "ModernSettingsForm");
                }
            };

            // Left panel - Fence list
            var leftPanel = new DarkGroupBox
            {
                Text = "Active Fences",
                Dock = DockStyle.Fill
            };

            fenceListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                BackColor = Color.FromArgb(60, 63, 65),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None
            };

            // Ensure columns are only added once and with sensible widths
            fenceListView.Columns.Clear();
            fenceListView.Columns.Add("Name", 200);
            fenceListView.Columns.Add("ID", 120);
            fenceListView.Columns.Add("Size", 80);
            fenceListView.SelectedIndexChanged += FenceListView_SelectedIndexChanged;

            var fenceButtonPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            var btnRefresh = new DarkButton
            {
                Text = "Refresh",
                Size = new Size(70, 25),
                Location = new Point(5, 8)
            };
            btnRefresh.Click += BtnRefresh_Click;

            var btnHighlight = new DarkButton
            {
                Text = "Highlight",
                Size = new Size(80, 25),
                Location = new Point(85, 8)
            };
            btnHighlight.Click += BtnHighlight_Click;

            var btnAdd = new DarkButton
            {
                Text = "Add",
                Size = new Size(60, 25),
                Location = new Point(175, 8)
            };
            btnAdd.Click += BtnAdd_Click;

            fenceButtonPanel.Controls.AddRange(new Control[] { btnRefresh, btnHighlight, btnAdd });
            leftPanel.Controls.Add(fenceListView);
            leftPanel.Controls.Add(fenceButtonPanel);

            // Right panel - Fence settings
            fenceSettingsGroup = new DarkGroupBox
            {
                Text = "Fence Settings",
                Dock = DockStyle.Fill,
                Enabled = false
            };

            CreateFenceSettingsControls();

            mainSplitter.Panel1.Controls.Add(leftPanel);
            mainSplitter.Panel2.Controls.Add(fenceSettingsGroup);
            fencesTab.Controls.Add(mainSplitter);
        }

        private void CreateFenceSettingsControls()
        {
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                AutoScroll = true,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            var contentPanel = new Panel
            {
                AutoSize = true,
                Width = scrollPanel.Width - 25,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            int yPos = 10;

            // Basic Settings
            AddTextSetting(contentPanel, "Name", "Fence Name", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "Transparency", "Transparency (%)", 25, 100, ref yPos, fenceSettings);
            AddBooleanSetting(contentPanel, "AutoHide", "Auto Hide", "Hide when not in use", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "AutoHideDelay", "Auto Hide Delay (ms)", 500, 10000, ref yPos, fenceSettings);
            AddBooleanSetting(contentPanel, "Locked", "Locked", "Prevent moving/resizing", ref yPos, fenceSettings);
            AddBooleanSetting(contentPanel, "CanMinify", "Can Minify", "Allow minimizing", ref yPos, fenceSettings);

            yPos += 20; // Space before size settings

            // Size Settings
            AddNumericSetting(contentPanel, "Width", "Width", 200, 2000, ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "Height", "Height", 200, 2000, ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "TitleHeight", "Title Height", 16, 100, ref yPos, fenceSettings);

            yPos += 20; // Space before color settings

            // Color Settings
            AddColorSetting(contentPanel, "BackgroundColor", "Background Color", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "BackgroundTransparency", "Background Transparency (%)", 0, 100, ref yPos, fenceSettings);
            AddColorSetting(contentPanel, "TitleBackgroundColor", "Title Background Color", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "TitleBackgroundTransparency", "Title Background Transparency (%)", 0, 100, ref yPos, fenceSettings);
            AddColorSetting(contentPanel, "TextColor", "Text Color", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "TextTransparency", "Text Transparency (%)", 0, 100, ref yPos, fenceSettings);
            AddColorSetting(contentPanel, "BorderColor", "Border Color", ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "BorderTransparency", "Border Transparency (%)", 0, 100, ref yPos, fenceSettings);

            yPos += 20; // Space before style settings

            // Style Settings
            AddNumericSetting(contentPanel, "BorderWidth", "Border Width", 0, 10, ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "CornerRadius", "Corner Radius", 0, 20, ref yPos, fenceSettings);
            AddBooleanSetting(contentPanel, "ShowShadow", "Show Shadow", "Display drop shadow", ref yPos, fenceSettings);
            AddComboBoxSetting(contentPanel, "IconSize", "Icon Size", new[] { "16", "24", "32", "48", "64" }, ref yPos, fenceSettings);
            AddNumericSetting(contentPanel, "ItemSpacing", "Item Spacing", 5, 50, ref yPos, fenceSettings);

            // Reset to defaults button
            yPos += 20;
            var btnResetToDefaults = new DarkButton
            {
                Text = "Reset to Defaults",
                Size = new Size(150, 30),
                Location = new Point(10, yPos)
            };
            btnResetToDefaults.Click += BtnResetToDefaults_Click;
            contentPanel.Controls.Add(btnResetToDefaults);

            // Set as defaults button
            var btnSetAsDefaults = new DarkButton
            {
                Text = "Set as Defaults",
                Size = new Size(150, 30),
                Location = new Point(170, yPos)
            };
            btnSetAsDefaults.Click += BtnSetAsDefaults_Click;
            contentPanel.Controls.Add(btnSetAsDefaults);

            contentPanel.Height = yPos + 50;

            // Setup auto-apply change handlers
            foreach (var control in fenceSettings.Values)
            {
                SetupAutoApplyHandler(control);
            }

            scrollPanel.Controls.Add(contentPanel);
            fenceSettingsGroup.Controls.Add(scrollPanel);
        }

        private DarkGroupBox CreateGroupBox(string title, ref int yPos, int width)
        {
            var groupBox = new DarkGroupBox
            {
                Text = title,
                Location = new Point(10, yPos),
                Width = width,
                AutoSize = false
            };
            
            return groupBox;
        }

        private void FinalizeGroupBox(DarkGroupBox groupBox, int contentHeight)
        {
            groupBox.Height = contentHeight + 25; // Add padding for group box border
        }

        private void AddBooleanSetting(Control parent, string propertyName, string displayName, string description, ref int yPos, Dictionary<string, Control> settingsDict = null)
        {
            if (settingsDict == null) settingsDict = globalSettings;

            var checkBox = new DarkCheckBox
            {
                Text = displayName,
                Location = new Point(10, yPos),
                AutoSize = true
            };

            if (!string.IsNullOrEmpty(description))
            {
                var toolTip = new ToolTip();
                toolTip.SetToolTip(checkBox, description);
            }

            settingsDict[propertyName] = checkBox;
            parent.Controls.Add(checkBox);
            yPos += 30;
        }

        private void AddNumericSetting(Control parent, string propertyName, string displayName, decimal min, decimal max, ref int yPos, Dictionary<string, Control> settingsDict = null)
        {
            if (settingsDict == null) settingsDict = globalSettings;

            var label = new DarkLabel
            {
                Text = displayName + ":",
                Location = new Point(10, yPos + 3),
                AutoSize = true
            };

            var numericUpDown = new DarkNumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Location = new Point(200, yPos),
                Width = 100
            };

            settingsDict[propertyName] = numericUpDown;
            parent.Controls.Add(label);
            parent.Controls.Add(numericUpDown);
            yPos += 30;
        }

        private void AddTextSetting(Control parent, string propertyName, string displayName, ref int yPos, Dictionary<string, Control> settingsDict = null)
        {
            if (settingsDict == null) settingsDict = globalSettings;

            var label = new DarkLabel
            {
                Text = displayName + ":",
                Location = new Point(10, yPos + 3),
                AutoSize = true
            };

            var textBox = new DarkTextBox
            {
                Location = new Point(200, yPos),
                Width = 200
            };

            settingsDict[propertyName] = textBox;
            parent.Controls.Add(label);
            parent.Controls.Add(textBox);
            yPos += 30;
        }

        private void AddComboBoxSetting(Control parent, string propertyName, string displayName, string[] items, ref int yPos, Dictionary<string, Control> settingsDict = null)
        {
            if (settingsDict == null) settingsDict = globalSettings;

            var label = new DarkLabel
            {
                Text = displayName + ":",
                Location = new Point(10, yPos + 3),
                AutoSize = true
            };

            var comboBox = new DarkComboBox
            {
                Location = new Point(200, yPos),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(items);

            settingsDict[propertyName] = comboBox;
            parent.Controls.Add(label);
            parent.Controls.Add(comboBox);
            yPos += 30;
        }

        private void AddColorSetting(Control parent, string propertyName, string displayName, ref int yPos, Dictionary<string, Control> settingsDict = null)
        {
            if (settingsDict == null) settingsDict = globalSettings;

            var label = new DarkLabel
            {
                Text = displayName + ":",
                Location = new Point(10, yPos + 3),
                AutoSize = true
            };

            var colorButton = new DarkButton
            {
                Text = "Choose Color",
                Location = new Point(200, yPos),
                Width = 120,
                Height = 23,
                Tag = propertyName
            };
            colorButton.Click += ColorButton_Click;

            settingsDict[propertyName] = colorButton;
            parent.Controls.Add(label);
            parent.Controls.Add(colorButton);
            yPos += 30;
        }

        private void AddHotkeySetting(Control parent, string propertyName, string displayName, ref int yPos)
        {
            var label = new DarkLabel
            {
                Text = displayName + ":",
                Location = new Point(10, yPos + 3),
                AutoSize = true
            };

            var textBox = new DarkTextBox
            {
                Location = new Point(200, yPos),
                Width = 150,
                ReadOnly = true
            };

            var btnClear = new DarkButton
            {
                Text = "Clear",
                Location = new Point(360, yPos),
                Width = 60,
                Height = 23
            };
            btnClear.Click += (s, e) => textBox.Text = "";

            globalSettings[propertyName] = textBox;
            parent.Controls.Add(label);
            parent.Controls.Add(textBox);
            parent.Controls.Add(btnClear);
            yPos += 30;
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            var button = sender as DarkButton;
            var propertyName = button.Tag as string;

            using (var colorDialog = new ColorDialog())
            {
                colorDialog.FullOpen = true;
                
                if (colorDialog.ShowDialog(this) == DialogResult.OK)
                {
                    button.BackColor = colorDialog.Color;
                    button.Text = $"#{colorDialog.Color.R:X2}{colorDialog.Color.G:X2}{colorDialog.Color.B:X2}";
                    
                    // Auto-apply if this is a fence setting
                    if (fenceSettings.ContainsValue(button))
                    {
                        ApplyFenceSettings();
                    }
                }
            }
        }

        private void SetupAutoApplyHandler(Control control)
        {
            if (control is DarkCheckBox checkBox)
            {
                checkBox.CheckedChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            }
            else if (control is DarkNumericUpDown numericUpDown)
            {
                numericUpDown.ValueChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            }
            else if (control is DarkTextBox textBox)
            {
                textBox.TextChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            }
            else if (control is DarkComboBox comboBox)
            {
                comboBox.SelectedIndexChanged += (s, e) => { if (!isUpdatingControls) ApplyFenceSettings(); };
            }
        }

        private void LoadSettings()
        {
            try
            {
                logger.Debug("Loading settings into modern settings form", "ModernSettingsForm");
                var settings = AppSettings.Instance;

                LoadSettingsToControls(settings, globalSettings);
                LoadSettingsToControls(settings, defaultSettings);

                logger.Info("Settings loaded successfully", "ModernSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load settings", "ModernSettingsForm", ex);
            }
        }

        private void LoadSettingsToControls(object settingsObject, Dictionary<string, Control> controlsDict)
        {
            var properties = settingsObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (controlsDict.TryGetValue(property.Name, out var control))
                {
                    var value = property.GetValue(settingsObject);
                    SetControlValue(control, value, property.PropertyType);
                }
            }
        }

        private void SetControlValue(Control control, object value, Type propertyType)
        {
            if (control is DarkCheckBox checkBox && value is bool boolValue)
            {
                checkBox.Checked = boolValue;
            }
            else if (control is DarkNumericUpDown numericUpDown && value != null)
            {
                numericUpDown.Value = Convert.ToDecimal(value);
            }
            else if (control is DarkTextBox textBox && value != null)
            {
                textBox.Text = value.ToString();
            }
            else if (control is DarkComboBox comboBox && value != null)
            {
                comboBox.SelectedItem = value.ToString();
            }
            else if (control is DarkButton colorButton && value != null && propertyType == typeof(int))
            {
                var color = Color.FromArgb((int)value);
                colorButton.BackColor = color;
                colorButton.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }

        private void LoadFences()
        {
            try
            {
                logger.Debug("Loading fences into fence management list", "ModernSettingsForm");
                
                fenceInfos = FenceManager.Instance.GetAllFenceInfos();
                
                Guid? selectedId = selectedFenceInfo?.Id;
                
                fenceListView.Items.Clear();
                
                for (int i = 0; i < fenceInfos.Count; i++)
                {
                    var fence = fenceInfos[i];
                    var item = new ListViewItem(new[] {
                        fence.Name,
                        fence.Id.ToString().Substring(0, 8) + "...",
                        $"{fence.Width}x{fence.Height}"
                    });
                    item.Tag = fence;
                    fenceListView.Items.Add(item);
                    
                    if (selectedId.HasValue && fence.Id == selectedId.Value)
                    {
                        item.Selected = true;
                    }
                }
                
                if (fenceListView.SelectedItems.Count == 0 && fenceListView.Items.Count > 0)
                {
                    fenceListView.Items[0].Selected = true;
                }

                logger.Info($"Loaded {fenceInfos.Count} fences", "ModernSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to load fences", "ModernSettingsForm", ex);
            }
        }

        private void FenceListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fenceListView.SelectedItems.Count > 0)
            {
                selectedFenceInfo = fenceListView.SelectedItems[0].Tag as FenceInfo;
                LoadFenceSettings();
                fenceSettingsGroup.Enabled = true;
            }
            else
            {
                selectedFenceInfo = null;
                fenceSettingsGroup.Enabled = false;
            }
        }

        private void LoadFenceSettings()
        {
            if (selectedFenceInfo == null) return;

            try
            {
                isUpdatingControls = true;
                LoadSettingsToControls(selectedFenceInfo, fenceSettings);
                isUpdatingControls = false;

                logger.Debug($"Loaded settings for fence '{selectedFenceInfo.Name}'", "ModernSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load fence settings for '{selectedFenceInfo?.Name}'", "ModernSettingsForm", ex);
                isUpdatingControls = false;
            }
        }

        private void ApplyFenceSettings()
        {
            if (selectedFenceInfo == null || isUpdatingControls) return;

            try
            {
                ApplyControlsToSettings(selectedFenceInfo, fenceSettings);
                
                FenceManager.Instance.UpdateFence(selectedFenceInfo);
                FenceManager.Instance.ApplySettingsToFence(selectedFenceInfo);
                
                // Update list view
                if (fenceListView.SelectedItems.Count > 0)
                {
                    var item = fenceListView.SelectedItems[0];
                    item.SubItems[0].Text = selectedFenceInfo.Name;
                    item.SubItems[2].Text = $"{selectedFenceInfo.Width}x{selectedFenceInfo.Height}";
                }

                logger.Info($"Applied settings to fence '{selectedFenceInfo.Name}'", "ModernSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to apply fence settings for '{selectedFenceInfo?.Name}'", "ModernSettingsForm", ex);
            }
        }

        private void ApplyControlsToSettings(object settingsObject, Dictionary<string, Control> controlsDict)
        {
            var properties = settingsObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (controlsDict.TryGetValue(property.Name, out var control) && property.CanWrite)
                {
                    var value = GetControlValue(control, property.PropertyType);
                    if (value != null)
                    {
                        property.SetValue(settingsObject, value);
                    }
                }
            }
        }

        private object GetControlValue(Control control, Type targetType)
        {
            if (control is DarkCheckBox checkBox)
            {
                return checkBox.Checked;
            }
            else if (control is DarkNumericUpDown numericUpDown)
            {
                if (targetType == typeof(int))
                    return (int)numericUpDown.Value;
                else if (targetType == typeof(decimal))
                    return numericUpDown.Value;
                else if (targetType == typeof(double))
                    return (double)numericUpDown.Value;
                else if (targetType == typeof(float))
                    return (float)numericUpDown.Value;
            }
            else if (control is DarkTextBox textBox)
            {
                return textBox.Text;
            }
            else if (control is DarkComboBox comboBox)
            {
                if (targetType == typeof(int) && int.TryParse(comboBox.SelectedItem?.ToString(), out int intResult))
                    return intResult;
                return comboBox.SelectedItem?.ToString();
            }
            else if (control is DarkButton colorButton && targetType == typeof(int))
            {
                return colorButton.BackColor.ToArgb();
            }

            return null;
        }

        private void ApplyGlobalSettings()
        {
            try
            {
                logger.Debug("Applying global settings", "ModernSettingsForm");
                var settings = AppSettings.Instance;
                
                ApplyControlsToSettings(settings, globalSettings);
                ApplyControlsToSettings(settings, defaultSettings);
                
                settings.SaveSettings();

                logger.Info("Global settings applied successfully", "ModernSettingsForm");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to apply global settings", "ModernSettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
                logger.Error("Failed to apply settings on OK", "ModernSettingsForm", ex);
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
                logger.Error("Failed to apply settings", "ModernSettingsForm", ex);
                MessageBox.Show($"Failed to apply settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadFences();
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
            var dialog = new ModernInputDialog("New Fence", "Enter fence name:");
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                FenceManager.Instance.CreateFence(dialog.InputText);
                LoadFences();
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
                    
                    // Apply default settings to fence
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
                    
                    // Copy fence settings to defaults
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                logger?.Debug("Disposing modern settings form", "ModernSettingsForm");
            }
            base.Dispose(disposing);
        }
    }
}