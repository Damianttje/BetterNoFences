using Fenceless.UI;
using System;
using System.Windows.Forms;

namespace Fenceless
{
    public class EditDialog : Form
    {
        private UI.EditDialog modernDialog;

        public EditDialog(string oldName)
        {
            modernDialog = new UI.EditDialog("Edit Name", oldName, "New name:");
        }

        public string NewName => modernDialog.NewName;

        public new DialogResult ShowDialog()
        {
            return modernDialog.ShowDialog();
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            return modernDialog.ShowDialog(owner);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                modernDialog?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
