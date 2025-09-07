using NoFences.Model;
using NoFences.Util;
using System;
using System.Threading;
using System.Windows.Forms;
using NoFences.Win32;

namespace NoFences
{
    static class Program
    {
        private static AutoSaveManager autoSaveManager;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //allows the context menu to be in dark mode
            //inherits from the system settings
            WindowUtil.SetPreferredAppMode(1);

            using (var mutex = new Mutex(true, "No_fences", out var createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Initialize auto-save manager
                    autoSaveManager = new AutoSaveManager();
                    
                    // Handle application exit
                    Application.ApplicationExit += Application_ApplicationExit;
                    
                    try
                    {
                        FenceManager.Instance.LoadFences();
                        if (Application.OpenForms.Count == 0)
                            FenceManager.Instance.CreateFence("First fence");

                        Application.Run();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Application error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                // Save all data before exit
                FenceManager.Instance.SaveAllFences();
                AppSettings.Instance.SaveSettings();
                autoSaveManager?.Dispose();
                FenceManager.Instance.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during application exit: {ex.Message}");
            }
        }
    }
}
