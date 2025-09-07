using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using NoFences.Util;
using System.Windows.Forms;
using System.Linq;

namespace NoFences.Model
{
    public class FenceManager
    {
        public static FenceManager Instance { get; } = new FenceManager();

        private const string MetaFileName = "__fence_metadata.xml";
        private readonly string basePath;
        private readonly List<FenceWindow> activeFences = new List<FenceWindow>();
        private GlobalHotkeyManager hotkeyManager;
        private int toggleAutoHideHotkeyId = -1;
        private int showAllFencesHotkeyId = -1;

        public FenceManager()
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NoFences");
            EnsureDirectoryExists(basePath);
            InitializeGlobalHotkeys();
        }

        private void InitializeGlobalHotkeys()
        {
            try
            {
                hotkeyManager = new GlobalHotkeyManager();
                RegisterGlobalHotkeys();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize global hotkeys: {ex.Message}");
            }
        }

        private void RegisterGlobalHotkeys()
        {
            try
            {
                // Unregister existing hotkeys
                if (toggleAutoHideHotkeyId != -1)
                {
                    hotkeyManager.UnregisterHotkey(toggleAutoHideHotkeyId);
                    toggleAutoHideHotkeyId = -1;
                }
                if (showAllFencesHotkeyId != -1)
                {
                    hotkeyManager.UnregisterHotkey(showAllFencesHotkeyId);
                    showAllFencesHotkeyId = -1;
                }

                // Register toggle auto-hide hotkey (Ctrl+Alt+H by default)
                toggleAutoHideHotkeyId = hotkeyManager.RegisterHotkey(
                    Keys.H, ctrl: true, alt: true, action: ToggleAllFencesAutoHide);

                // Register show all fences hotkey (Ctrl+Alt+S by default)
                showAllFencesHotkeyId = hotkeyManager.RegisterHotkey(
                    Keys.S, ctrl: true, alt: true, action: ShowAllFences);

                Console.WriteLine("Global hotkeys registered successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register global hotkeys: {ex.Message}");
            }
        }

        public void LoadFences()
        {
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(basePath))
                {
                    var metaFile = Path.Combine(dir, MetaFileName);
                    if (!File.Exists(metaFile))
                        continue;

                    try
                    {
                        var fenceInfo = LoadFenceInfo(metaFile);
                        if (fenceInfo != null)
                        {
                            var fenceWindow = new FenceWindow(fenceInfo);
                            activeFences.Add(fenceWindow);
                            fenceWindow.FormClosed += (s, e) => activeFences.Remove(fenceWindow);
                            fenceWindow.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue loading other fences
                        Console.WriteLine($"Failed to load fence from {dir}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load fences: {ex.Message}");
            }
        }

        private FenceInfo LoadFenceInfo(string metaFile)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(FenceInfo));
                using (var reader = new StreamReader(metaFile))
                {
                    return serializer.Deserialize(reader) as FenceInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to deserialize fence metadata from {metaFile}: {ex.Message}");
                return null;
            }
        }

        public void CreateFence(string name)
        {
            try
            {
                var settings = AppSettings.Instance;
                var fenceInfo = new FenceInfo(Guid.NewGuid())
                {
                    Name = name,
                    PosX = 100,
                    PosY = 250,
                    Height = settings.DefaultFenceHeight,
                    Width = settings.DefaultFenceWidth,
                    Transparency = settings.DefaultTransparency,
                    AutoHide = settings.DefaultAutoHide,
                    AutoHideDelay = settings.DefaultAutoHideDelay
                };

                UpdateFence(fenceInfo);
                var fenceWindow = new FenceWindow(fenceInfo);
                activeFences.Add(fenceWindow);
                fenceWindow.FormClosed += (s, e) => activeFences.Remove(fenceWindow);
                fenceWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create fence '{name}': {ex.Message}");
            }
        }

        public void RemoveFence(FenceInfo info)
        {
            try
            {
                var folderPath = GetFolderPath(info);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove fence {info.Name}: {ex.Message}");
            }
        }

        public void UpdateFence(FenceInfo fenceInfo)
        {
            try
            {
                var path = GetFolderPath(fenceInfo);
                EnsureDirectoryExists(path);

                var metaFile = Path.Combine(path, MetaFileName);
                var serializer = new XmlSerializer(typeof(FenceInfo));
                
                using (var writer = new StreamWriter(metaFile))
                {
                    serializer.Serialize(writer, fenceInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update fence {fenceInfo.Name}: {ex.Message}");
            }
        }

        private void EnsureDirectoryExists(string dir)
        {
            try
            {
                var di = new DirectoryInfo(dir);
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create directory {dir}: {ex.Message}");
            }
        }

        private string GetFolderPath(FenceInfo fenceInfo)
        {
            return Path.Combine(basePath, fenceInfo.Id.ToString());
        }

        public void SaveAllFences()
        {
            foreach (var fence in activeFences.ToList())
            {
                try
                {
                    var fenceInfo = fence.GetFenceInfo();
                    UpdateFence(fenceInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save fence: {ex.Message}");
                }
            }
        }

        // Global hotkey actions
        private void ToggleAllFencesAutoHide()
        {
            try
            {
                foreach (var fence in activeFences.ToList())
                {
                    var fenceInfo = fence.GetFenceInfo();
                    fenceInfo.AutoHide = !fenceInfo.AutoHide;
                    fence.UpdateAutoHideState();
                    UpdateFence(fenceInfo);
                }
                
                var status = activeFences.Count > 0 && activeFences[0].GetFenceInfo().AutoHide ? "enabled" : "disabled";
                Console.WriteLine($"Auto-hide {status} for all fences");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to toggle auto-hide for all fences: {ex.Message}");
            }
        }

        public void ShowAllFences()
        {
            try
            {
                foreach (var fence in activeFences.ToList())
                {
                    fence.ForceShow();
                }
                Console.WriteLine("All fences shown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to show all fences: {ex.Message}");
            }
        }

        public void ApplySettingsToAllFences(int transparency, bool autoHide, int autoHideDelay)
        {
            try
            {
                foreach (var fence in activeFences.ToList())
                {
                    var fenceInfo = fence.GetFenceInfo();
                    fenceInfo.Transparency = transparency;
                    fenceInfo.AutoHide = autoHide;
                    fenceInfo.AutoHideDelay = autoHideDelay;
                    
                    fence.ApplySettings();
                    UpdateFence(fenceInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to apply settings to all fences: {ex.Message}");
            }
        }

        public void ShowGlobalSettings()
        {
            try
            {
                var settingsForm = new SettingsForm();
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh global hotkeys if they changed
                    RegisterGlobalHotkeys();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to show global settings: {ex.Message}");
            }
        }

        public void ShowFenceSettings(FenceInfo fenceInfo)
        {
            try
            {
                var settingsForm = new SettingsForm(fenceInfo);
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Find the fence window and refresh its settings
                    var fenceWindow = activeFences.FirstOrDefault(f => f.GetFenceInfo().Id == fenceInfo.Id);
                    fenceWindow?.ApplySettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to show fence settings: {ex.Message}");
            }
        }

        public List<FenceInfo> GetAllFenceInfos()
        {
            return activeFences.Select(f => f.GetFenceInfo()).ToList();
        }

        public int GetActiveFenceCount()
        {
            return activeFences.Count;
        }

        public void Dispose()
        {
            try
            {
                hotkeyManager?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing FenceManager: {ex.Message}");
            }
        }
    }
}
