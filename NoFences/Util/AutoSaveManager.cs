using System;
using System.Threading;
using System.Threading.Tasks;
using Fenceless.Model;

namespace Fenceless.Util
{
    public class AutoSaveManager : IDisposable
    {
        private readonly Timer autoSaveTimer;
        private bool disposed = false;
        
        public AutoSaveManager()
        {
            var interval = TimeSpan.FromSeconds(AppSettings.Instance.AutoSaveInterval);
            autoSaveTimer = new Timer(AutoSave, null, interval, interval);
        }
        
        private void AutoSave(object state)
        {
            if (AppSettings.Instance.AutoSave)
            {
                Task.Run(() =>
                {
                    try
                    {
                        FenceManager.Instance.SaveAllFences();
                        AppSettings.Instance.SaveSettings();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Auto-save failed: {ex.Message}");
                    }
                });
            }
        }
        
        public void Dispose()
        {
            if (!disposed)
            {
                autoSaveTimer?.Dispose();
                disposed = true;
            }
        }
    }
}