using NoFences.UI;
using NoFences.Util;
using System;
using System.Windows.Forms;

namespace NoFences
{
    public partial class LogViewerForm : Form
    {
        private UI.LogViewerForm modernLogViewer;
        private readonly Logger logger;

        public LogViewerForm()
        {
            logger = Logger.Instance;
            modernLogViewer = new UI.LogViewerForm();
        }

        public new void Show()
        {
            modernLogViewer.Show();
        }

        public new void Hide()
        {
            modernLogViewer.Hide();
        }

        public new void BringToFront()
        {
            modernLogViewer.BringToFront();
        }

        public new bool Visible => modernLogViewer.Visible;

        public new bool IsDisposed => modernLogViewer.IsDisposed;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                modernLogViewer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}