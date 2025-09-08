using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NoFences.UI
{
    public partial class ModernEditDialog : DarkForm
    {
        private DarkLabel promptLabel;
        private DarkTextBox nameTextBox;
        private DarkButton btnOK;
        private DarkButton btnCancel;

        public string NewName => nameTextBox.Text;

        public ModernEditDialog(string title, string currentName = "", string prompt = "Name:")
        {
            InitializeComponent();
            this.Text = title;
            promptLabel.Text = prompt;
            nameTextBox.Text = currentName;
            nameTextBox.SelectAll();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Size = new Size(420, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;

            // Controls
            promptLabel = new DarkLabel
            {
                Location = new Point(20, 25),
                Size = new Size(350, 20),
                Text = "Name:",
                AutoSize = true
            };

            nameTextBox = new DarkTextBox
            {
                Location = new Point(20, 55),
                Size = new Size(360, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Button panel with dark styling
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(60, 63, 65)
            };

            btnCancel = new DarkButton
            {
                Text = "Cancel",
                Size = new Size(80, 30),
                Location = new Point(300, 10),
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnOK = new DarkButton
            {
                Text = "OK",
                Size = new Size(80, 30),
                Location = new Point(210, 10),
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            buttonPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });

            // Main content panel
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 63, 65)
            };
            contentPanel.Controls.AddRange(new Control[] { promptLabel, nameTextBox });

            // Add panels to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);

            // Set default button behavior
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;

            // Event handlers
            btnOK.Click += BtnOK_Click;
            nameTextBox.KeyDown += NameTextBox_KeyDown;

            this.ResumeLayout(false);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid name.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnOK_Click(sender, e);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            nameTextBox.Focus();
            nameTextBox.SelectAll();
        }
    }
}