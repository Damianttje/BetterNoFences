using NoFences.UI;
using NoFences.Util;
using System;
using System.Windows.Forms;

namespace NoFences
{
    public partial class GlobalSettingsForm : Form
    {
        private SettingsForm modernSettingsForm;
        private readonly Logger logger;

        public GlobalSettingsForm()
        {
            logger = Logger.Instance;
            logger.Debug("Creating global settings form wrapper", "GlobalSettingsForm");
            
            modernSettingsForm = new SettingsForm();
        }

        public new DialogResult ShowDialog()
        {
            return modernSettingsForm.ShowDialog();
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            return modernSettingsForm.ShowDialog(owner);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                modernSettingsForm?.Dispose();
                logger?.Debug("Disposing global settings form wrapper", "GlobalSettingsForm");
            }
            base.Dispose(disposing);
        }
    }
}