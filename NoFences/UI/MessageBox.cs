using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI;
using DarkUI.Controls;
using DarkUI.Data.Enums;
using DarkUI.Forms;

namespace NoFences.UI
{
    public class CustomMessageBox : DarkForm
    {
        public CustomMessageBox(string message, string title = "Fenceless | Message", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            // Create custom message box form
            this.Text = title;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Padding = new Padding(0);

            // Create header panel
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48)
            };

            // Create title label
            headerPanel.Controls.Add(new Label {Text = title, ForeColor = System.Drawing.Color.White, Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0)});
            this.Controls.Add(headerPanel);

            // Controlbox buttons
            DarkTitleBar titleBar = new DarkTitleBar(this, DarkTitleBarStyle.Standard)
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48)
            };

        }

        public static DialogResult Show(string message, string title = "Fenceless | Message", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (CustomMessageBox msgBox = new CustomMessageBox(message, title, buttons, icon))
            {
                return msgBox.ShowDialog();
            }
        }
    }
}
