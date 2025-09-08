using NoFences.UI;
using System;
using System.Windows.Forms;

namespace NoFences
{
    public class EditDialog : Form
    {
        private ModernEditDialog modernDialog;

        public EditDialog(string oldName)
        {
            modernDialog = new ModernEditDialog("Edit Name", oldName, "New name:");
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
