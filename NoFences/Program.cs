using NoFences.Model;
using NoFences.Win32;
using NoFences.Util;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace NoFences
{
    static class Program
    {
        private static Logger logger;
        private static LogViewerForm logViewerForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Initialize logging first
                logger = Logger.Instance;
                
                // Load settings to configure logging properly
                var settings = AppSettings.Instance;
                
                logger.Info("BetterNoFences application starting...", "Main");

                //allows the context menu to be in dark mode
                //inherits from the system settings
                WindowUtil.SetPreferredAppMode(1);

                using (var mutex = new Mutex(true, "No_fences", out var createdNew))
                {
                    if (createdNew)
                    {
                        logger.Info("Application mutex acquired, starting main application", "Main");
                        
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);

                        // Handle application exit
                        Application.ApplicationExit += Application_ApplicationExit;

                        // Handle unhandled exceptions
                        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                        Application.ThreadException += Application_ThreadException;
                        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                        var trayIcon = new NotifyIcon();
                        try
                        {
                            using (var ms = new MemoryStream(Properties.Resources.AppIconIco))
                            {
                                trayIcon.Icon = new Icon(ms);
                            }
                            trayIcon.Visible = true;
                            trayIcon.Text = "BetterNoFences - Desktop organization tool";

                            var contextMenu = new ContextMenuStrip();

                            // Add Add Fence menu item
                            var addFenceMenuItem = new ToolStripMenuItem("Add Fence");
                            addFenceMenuItem.Click += (s, e) =>
                            {
                                logger.Info("Add Fence requested from tray menu", "Main");
                                FenceManager.Instance.CreateFence("New Fence");
                            };
                            contextMenu.Items.Add(addFenceMenuItem);
                            
                            // Add Log Viewer menu item
                            var logViewerMenuItem = new ToolStripMenuItem("View Logs");
                            logViewerMenuItem.Click += (s, e) => ShowLogViewer();
                            contextMenu.Items.Add(logViewerMenuItem);
                            
                            contextMenu.Items.Add(new ToolStripSeparator());
                            
                            var exitMenuItem = new ToolStripMenuItem("Exit");
                            exitMenuItem.Click += (s, e) =>
                            {
                                logger.Info("Exit requested from tray menu", "Main");
                                trayIcon.Visible = false;
                                Application.Exit();
                            };
                            contextMenu.Items.Add(exitMenuItem);
                            trayIcon.ContextMenuStrip = contextMenu;

                            trayIcon.DoubleClick += (s, e) =>
                            {
                                logger.Debug("Tray icon double-clicked", "Main");
                                MessageBox.Show("BetterNoFences is running in the background.\n\nRight-click the tray icon to access options or view logs.", "NoFences", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            };

                            try
                            {
                                logger.Info("Loading fences...", "Main");
                                FenceManager.Instance.LoadFences();
                                if (Application.OpenForms.Count == 0)
                                {
                                    logger.Info("No existing fences found, creating first fence", "Main");
                                    FenceManager.Instance.CreateFence("First fence");
                                }
                                
                                logger.Info("BetterNoFences initialized successfully", "Main");
                                Application.Run();
                            }
                            catch (Exception ex)
                            {
                                logger.Critical("Application error during initialization", "Main", ex);
                                MessageBox.Show($"Application error: {ex.Message}\n\nPlease check the log files for more details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        finally
                        {
                            logger.Debug("Disposing tray icon", "Main");
                            trayIcon?.Dispose();
                        }
                    }
                    else
                    {
                        logger.Warning("Another instance of BetterNoFences is already running", "Main");
                        MessageBox.Show("BetterNoFences is already running.", "BetterNoFences", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Critical("Critical error in main application", "Main", ex);
                else
                    System.Diagnostics.Debug.WriteLine($"Critical error: {ex}");
                
                MessageBox.Show($"Critical application error: {ex.Message}\n\nPlease check the log files for more details.", "Critical Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            logger.Error("Unhandled thread exception", "Exception", e.Exception);
            
            var result = MessageBox.Show(
                $"An unexpected error occurred:\n{e.Exception.Message}\n\nWould you like to continue running the application?\n\nCheck the log viewer for more details.",
                "Unexpected Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            
            if (result == DialogResult.No)
            {
                Application.Exit();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Critical("Unhandled domain exception", "Exception", e.ExceptionObject as Exception);
            
            if (e.IsTerminating)
            {
                logger.Critical("Application is terminating due to unhandled exception", "Exception");
                MessageBox.Show(
                    $"A critical error occurred and the application must close:\n{e.ExceptionObject}\n\nPlease check the log files for more details.",
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowLogViewer()
        {
            try
            {
                logger.Debug("Log viewer requested", "Main");
                if (logViewerForm == null || logViewerForm.IsDisposed)
                {
                    logViewerForm = new LogViewerForm();
                }
                
                if (logViewerForm.Visible)
                {
                    logViewerForm.BringToFront();
                }
                else
                {
                    logViewerForm.Show();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to show log viewer", "Main", ex);
                MessageBox.Show($"Failed to show log viewer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                logger.Info("BetterNoFences shutting down...", "Main");
                
                // Save all data before exit
                FenceManager.Instance.SaveAllFences();
                AppSettings.Instance.SaveSettings();
                FenceManager.Instance.Dispose();
                
                logger.Info("BetterNoFences shutdown complete", "Main");

                // Flush and dispose logger
                logger.FlushLogs();
                logger.Dispose();
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Error("Error during application exit", "Main", ex);
                else
                    System.Diagnostics.Debug.WriteLine($"Error during application exit: {ex.Message}");
            }
        }
    }
}
