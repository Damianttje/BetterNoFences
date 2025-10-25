using System;
using System.Drawing;
using System.Windows.Forms;
using Fenceless.Util;

namespace Fenceless.UI
{
    public partial class InputDialog : Form
    {
        #region Control Declarations
        private Panel mainPanel;
        private Label titleLabel;
        private Label promptLabel;
        private TextBox inputTextBox;
        private Button okButton;
        private Button cancelButton;
        #endregion

        #region Constants - Application Colors
        private static readonly Color AppBackgroundColor = Color.FromArgb(60, 63, 65);
        private static readonly Color TextColor = Color.FromArgb(220, 220, 220);
        private static readonly Color ButtonBackColor = Color.FromArgb(70, 73, 75);
        private static readonly Color ButtonForeColor = Color.FromArgb(220, 220, 220);
        private static readonly Color TextBoxBackColor = Color.FromArgb(50, 53, 55);
        private static readonly Color TextBoxForeColor = Color.FromArgb(220, 220, 220);
        #endregion

        #region Properties
        public string InputText => inputTextBox?.Text ?? string.Empty;
        public string Title { get; private set; }
        public string Prompt { get; private set; }
        public string DefaultValue { get; private set; }
        #endregion

        #region Constructor
        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            Title = title ?? "Input";
            Prompt = prompt ?? "Please enter a value:";
            DefaultValue = defaultValue ?? string.Empty;

            InitializeComponent();
        }
        #endregion

        #region Form Initialization
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup - match SettingsForm style
            this.Text = Title;
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(350, 150);
            this.MaximumSize = new Size(600, 300);

            CreateControls();
            SetupEventHandlers();

            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Main panel with app background color
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppBackgroundColor,
                Padding = new Padding(20)
            };

            // Title label
            titleLabel = new Label
            {
                Text = Title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = TextColor,
                Location = new Point(0, 0),
                Size = new Size(mainPanel.Width - 40, 30),
                AutoSize = false,
                BackColor = Color.Transparent
            };

            // Prompt label
            promptLabel = new Label
            {
                Text = Prompt,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextColor,
                Location = new Point(0, 40),
                Size = new Size(mainPanel.Width - 40, 20),
                AutoSize = false,
                BackColor = Color.Transparent
            };

            // Input text box
            inputTextBox = new TextBox
            {
                Location = new Point(0, 65),
                Size = new Size(mainPanel.Width - 40, 23),
                Font = new Font("Segoe UI", 9F),
                BackColor = TextBoxBackColor,
                ForeColor = TextBoxForeColor,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Button panel at bottom
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = AppBackgroundColor
            };

            // OK button
            okButton = new Button
            {
                Text = "OK",
                Size = new Size(80, 30),
                Location = new Point(buttonPanel.Width - 170, 10),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = ButtonBackColor,
                ForeColor = ButtonForeColor,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.OK
            };
            okButton.FlatAppearance.BorderSize = 1;
            okButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);

            // Cancel button
            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                Location = new Point(buttonPanel.Width - 80, 10),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = ButtonBackColor,
                ForeColor = ButtonForeColor,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                DialogResult = DialogResult.Cancel
            };
            cancelButton.FlatAppearance.BorderSize = 1;
            cancelButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);

            buttonPanel.Controls.AddRange(new Control[] { okButton, cancelButton });
            mainPanel.Controls.AddRange(new Control[] { titleLabel, promptLabel, inputTextBox });

            // Add panels to form
            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            // Set default button behavior
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
        #endregion

        #region Event Handlers
        private void SetupEventHandlers()
        {
            okButton.Click += OkButton_Click;
            inputTextBox.KeyDown += InputTextBox_KeyDown;
            this.Load += InputDialog_Load;
            this.Shown += InputDialog_Shown;
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {
            try
            {
                inputTextBox.Text = DefaultValue;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error loading InputDialog: {ex.Message}", "InputDialog.Load");
            }
        }

        private void InputDialog_Shown(object sender, EventArgs e)
        {
            try
            {
                inputTextBox.Focus();
                inputTextBox.SelectAll();
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error showing InputDialog: {ex.Message}", "InputDialog.Shown");
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateInput())
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error in OK button click: {ex.Message}", "InputDialog.OkButtonClick");
                ErrorHandler.HandleException(ex, "Failed to process input", "InputDialog.OkButtonClick");
            }
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    OkButton_Click(sender, e);
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error handling key down: {ex.Message}", "InputDialog.KeyDown");
            }
        }
        #endregion

        #region Validation
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a valid value.",
                    "Invalid Input",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                
                inputTextBox.Focus();
                inputTextBox.SelectAll();
                return false;
            }

            return true;
        }
        #endregion

        #region Public Methods
        public static string ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            try
            {
                using (var dialog = new InputDialog(title, prompt, defaultValue))
                {
                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK ? dialog.InputText : string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error showing input dialog: {ex.Message}", "InputDialog.ShowDialog");
                ErrorHandler.HandleException(ex, "Failed to show input dialog", "InputDialog.ShowDialog");
                return string.Empty;
            }
        }

        public static string ShowInputDialog(IWin32Window owner, string title, string prompt, string defaultValue = "")
        {
            try
            {
                using (var dialog = new InputDialog(title, prompt, defaultValue))
                {
                    var result = dialog.ShowDialog(owner);
                    return result == DialogResult.OK ? dialog.InputText : string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error showing input dialog with owner: {ex.Message}", "InputDialog.ShowDialogWithOwner");
                ErrorHandler.HandleException(ex, "Failed to show input dialog", "InputDialog.ShowDialogWithOwner");
                return string.Empty;
            }
        }
        #endregion

        #region Cleanup
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    mainPanel?.Dispose();
                    titleLabel?.Dispose();
                    promptLabel?.Dispose();
                    inputTextBox?.Dispose();
                    okButton?.Dispose();
                    cancelButton?.Dispose();
                }
                base.Dispose(disposing);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Error disposing InputDialog: {ex.Message}", "InputDialog.Dispose");
            }
        }
        #endregion
    }
}