using NoFences.Model;
using System;
using System.Windows.Forms;

namespace NoFences
{
    public partial class SettingsForm : Form
    {
        private readonly FenceInfo selectedFenceInfo;
        private readonly bool isGlobalSettings;

        // Controls
        private TabControl tabControl;
        private TabPage tabGeneral;
        private TabPage tabAppearance;
        private Button btnOK;
        private Button btnCancel;
        
        // Fence settings controls
        private TextBox txtFenceName;
        private NumericUpDown numTransparency;
        private CheckBox chkAutoHide;
        private NumericUpDown numAutoHideDelay;
        private CheckBox chkLocked;
        private CheckBox chkCanMinify;

        public SettingsForm() : this(null, true)
        {
        }

        public SettingsForm(FenceInfo fenceInfo) : this(fenceInfo, false)
        {
        }

        private SettingsForm(FenceInfo fenceInfo, bool globalSettings)
        {
            selectedFenceInfo = fenceInfo;
            isGlobalSettings = globalSettings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.tabControl = new TabControl();
            this.tabGeneral = new TabPage();
            this.tabAppearance = new TabPage();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            
            // Form setup
            this.Text = isGlobalSettings ? "Global Settings" : $"Settings for '{selectedFenceInfo?.Name}'";
            this.Size = new System.Drawing.Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // Tab control
            this.tabControl.Dock = DockStyle.Fill;
            this.tabControl.Margin = new Padding(12, 12, 12, 50);
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabAppearance);
            
            // Tab pages
            this.tabGeneral.Text = "General";
            this.tabAppearance.Text = "Appearance";
            
            // Buttons
            this.btnOK.Text = "OK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.Location = new System.Drawing.Point(338, 330);
            this.btnOK.Click += btnOK_Click;
            
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.Location = new System.Drawing.Point(419, 330);
            this.btnCancel.Click += btnCancel_Click;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            
            // Add controls to form
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            
            if (!isGlobalSettings)
            {
                SetupFenceControls();
            }
            else
            {
                SetupGlobalControls();
            }
        }

        private void SetupFenceControls()
        {
            // Name
            var lblName = new Label { Text = "Name:", Location = new System.Drawing.Point(10, 20) };
            this.txtFenceName = new TextBox { Location = new System.Drawing.Point(100, 18), Width = 200 };
            
            // Transparency
            var lblTransparency = new Label { Text = "Transparency:", Location = new System.Drawing.Point(10, 20) };
            this.numTransparency = new NumericUpDown 
            { 
                Location = new System.Drawing.Point(100, 18), 
                Minimum = 25, 
                Maximum = 100, 
                Value = 100 
            };
            
            // Auto Hide
            this.chkAutoHide = new CheckBox { Text = "Auto Hide", Location = new System.Drawing.Point(10, 50) };
            this.numAutoHideDelay = new NumericUpDown 
            { 
                Location = new System.Drawing.Point(100, 48), 
                Minimum = 500, 
                Maximum = 10000, 
                Value = 2000 
            };
            
            // Other options
            this.chkLocked = new CheckBox { Text = "Locked", Location = new System.Drawing.Point(10, 80) };
            this.chkCanMinify = new CheckBox { Text = "Can Minify", Location = new System.Drawing.Point(10, 110) };
            
            // Add to tabs
            this.tabGeneral.Controls.AddRange(new Control[] { lblName, txtFenceName, chkLocked, chkCanMinify });
            this.tabAppearance.Controls.AddRange(new Control[] { lblTransparency, numTransparency, chkAutoHide, numAutoHideDelay });
        }

        private void SetupGlobalControls()
        {
            // Global settings controls
            var btnShowAllFences = new Button 
            { 
                Text = "Show All Fences", 
                Location = new System.Drawing.Point(10, 60),
                Size = new System.Drawing.Size(150, 30)
            };
            btnShowAllFences.Click += (s, e) => FenceManager.Instance.ShowAllFences();

            var lblInfo = new Label 
            { 
                Text = "Global hotkeys:\nCtrl+Alt+H - Toggle auto-hide\nCtrl+Alt+S - Show all fences\nCtrl+Alt+T - Toggle transparency", 
                Location = new System.Drawing.Point(10, 100),
                Size = new System.Drawing.Size(400, 80),
                AutoSize = false
            };

            this.tabGeneral.Controls.AddRange(new Control[] { btnShowAllFences, lblInfo });
        }

        private void LoadSettings()
        {
            if (isGlobalSettings)
            {
                // Load global settings
                Text = "BetterNoFences - Global Settings";
            }
            else if (selectedFenceInfo != null)
            {
                // Load fence settings
                Text = $"Settings for '{selectedFenceInfo.Name}'";
                txtFenceName.Text = selectedFenceInfo.Name;
                numTransparency.Value = selectedFenceInfo.Transparency;
                chkAutoHide.Checked = selectedFenceInfo.AutoHide;
                numAutoHideDelay.Value = selectedFenceInfo.AutoHideDelay;
                chkLocked.Checked = selectedFenceInfo.Locked;
                chkCanMinify.Checked = selectedFenceInfo.CanMinify;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (isGlobalSettings)
                {
                    SaveGlobalSettings();
                }
                else
                {
                    SaveFenceSettings();
                }
                
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveGlobalSettings()
        {
            // Global settings implementation
            AppSettings.Instance.SaveSettings();
        }

        private void SaveFenceSettings()
        {
            if (selectedFenceInfo != null)
            {
                var oldName = selectedFenceInfo.Name;
                var newName = txtFenceName.Text;
                
                selectedFenceInfo.Name = newName;
                selectedFenceInfo.Transparency = (int)numTransparency.Value;
                selectedFenceInfo.AutoHide = chkAutoHide.Checked;
                selectedFenceInfo.AutoHideDelay = (int)numAutoHideDelay.Value;
                selectedFenceInfo.Locked = chkLocked.Checked;
                selectedFenceInfo.CanMinify = chkCanMinify.Checked;
                
                FenceManager.Instance.UpdateFence(selectedFenceInfo);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            base.Dispose(disposing);
        }
    }
}