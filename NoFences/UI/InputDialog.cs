using DarkUI.Controls;
using DarkUI.Forms;
using Fenceless.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fenceless.UI
{
    public partial class InputDialog : DarkForm
    {
        private DarkLabel promptLabel;
        private DarkTextBox inputTextBox;
        private DarkButton btnOK;
        private DarkButton btnCancel;

        public string InputText => inputTextBox.Text;

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent();
            this.Text = title;
            promptLabel.Text = prompt;
            inputTextBox.Text = defaultValue;
            inputTextBox.SelectAll();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Size = new Size(400, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;

            // Controls
            promptLabel = new DarkLabel
            {
                Location = new Point(20, 20),
                Size = new Size(350, 20),
                Text = "Prompt:",
                AutoSize = true
            };

            inputTextBox = new DarkTextBox
            {
                Location = new Point(20, 50),
                Size = new Size(340, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Button panel with dark styling
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            btnOK = new DarkButton
            {
                Text = "OK",
                Size = new Size(80, 30),
                Location = new Point(200, 10),
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnCancel = new DarkButton
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                Location = new Point(290, 10),
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            buttonPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });

            // Main content panel
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 63, 65)
            };
            contentPanel.Controls.AddRange(new Control[] { promptLabel, inputTextBox });

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);

            // Set default button behavior
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Event handlers
            btnOK.Click += BtnOK_Click;
            inputTextBox.KeyDown += InputTextBox_KeyDown;

            this.ResumeLayout(false);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputTextBox.Text))
            {
                MessageBox.Show("Please enter a valid value.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                inputTextBox.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnOK_Click(sender, e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            inputTextBox.Focus();
            inputTextBox.SelectAll();
        }
    }
}