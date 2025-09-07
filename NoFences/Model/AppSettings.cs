using Newtonsoft.Json;
using System;
using System.IO;

namespace NoFences.Model
{
    public class AppSettings
    {
        public static AppSettings Instance { get; } = new AppSettings();
        
        public bool AutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 30; // seconds
        public bool ShowTooltips { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public string Theme { get; set; } = "Auto";
        public int DefaultFenceWidth { get; set; } = 300;
        public int DefaultFenceHeight { get; set; } = 300;
        
        // New transparency and autohide settings
        public int DefaultTransparency { get; set; } = 100;
        public bool DefaultAutoHide { get; set; } = false;
        public int DefaultAutoHideDelay { get; set; } = 2000;
        
        // Global keyboard shortcuts
        public string ToggleTransparencyShortcut { get; set; } = "Ctrl+Alt+T";
        public string ToggleAutoHideShortcut { get; set; } = "Ctrl+Alt+H";
        public string ShowAllFencesShortcut { get; set; } = "Ctrl+Alt+S";
        
        private readonly string settingsPath;
        
        private AppSettings()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NoFences");
            settingsPath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
        }
        
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (settings != null)
                    {
                        AutoSave = settings.AutoSave;
                        AutoSaveInterval = settings.AutoSaveInterval;
                        ShowTooltips = settings.ShowTooltips;
                        EnableAnimations = settings.EnableAnimations;
                        Theme = settings.Theme;
                        DefaultFenceWidth = settings.DefaultFenceWidth;
                        DefaultFenceHeight = settings.DefaultFenceHeight;
                        DefaultTransparency = settings.DefaultTransparency;
                        DefaultAutoHide = settings.DefaultAutoHide;
                        DefaultAutoHideDelay = settings.DefaultAutoHideDelay;
                        ToggleTransparencyShortcut = settings.ToggleTransparencyShortcut ?? "Ctrl+Alt+T";
                        ToggleAutoHideShortcut = settings.ToggleAutoHideShortcut ?? "Ctrl+Alt+H";
                        ShowAllFencesShortcut = settings.ShowAllFencesShortcut ?? "Ctrl+Alt+S";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}