using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;
using NoFences.Util;

namespace NoFences.Model
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(() => new AppSettings());
        public static AppSettings Instance => _instance.Value;

        public bool AutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 30; // seconds
        public bool ShowTooltips { get; set; } = true;
        public bool EnableAnimations { get; set; } = true;
        public int DefaultFenceWidth { get; set; } = 524;
        public int DefaultFenceHeight { get; set; } = 517;

        // New transparency and autohide settings
        public int DefaultTransparency { get; set; } = 80;
        public bool DefaultAutoHide { get; set; } = false;
        public int DefaultAutoHideDelay { get; set; } = 2000;
        public int DefaultTitleHeight { get; set; } = 25;

        // Default color settings
        public int DefaultBackgroundColor { get; set; } = -12498056;
        public int DefaultTitleBackgroundColor { get; set; } = -13551525;
        public int DefaultTextColor { get; set; } = -1;
        public int DefaultBorderColor { get; set; } = -8355712;
        public int DefaultBorderWidth { get; set; } = 0;
        public int DefaultCornerRadius { get; set; } = 0;
        public bool DefaultShowShadow { get; set; } = true;
        public int DefaultIconSize { get; set; } = 32;
        public int DefaultItemSpacing { get; set; } = 15;

        // Default transparency settings for color components
        public int DefaultBackgroundTransparency { get; set; } = 50;
        public int DefaultTitleBackgroundTransparency { get; set; } = 75;
        public int DefaultTextTransparency { get; set; } = 100;
        public int DefaultBorderTransparency { get; set; } = 100;

        // Logging settings
        public string LogLevel { get; set; } = "Info";
        public bool EnableFileLogging { get; set; } = true;

        // Global keyboard shortcuts
        public string ToggleTransparencyShortcut { get; set; } = "Ctrl+Alt+T";
        public string ToggleAutoHideShortcut { get; set; } = "Ctrl+Alt+H";
        public string ShowAllFencesShortcut { get; set; } = "Ctrl+Alt+S";
        public string CreateNewFenceShortcut { get; set; } = "Ctrl+Alt+N";
        public string OpenSettingsShortcut { get; set; } = "Ctrl+Alt+O";
        public string ToggleLockShortcut { get; set; } = "Ctrl+Alt+L";
        public string MinimizeAllFencesShortcut { get; set; } = "Ctrl+Alt+M";
        public string RefreshFencesShortcut { get; set; } = "F5";

        [JsonIgnore]
        private readonly string settingsPath;
        
        [JsonIgnore]
        private readonly Logger logger;
        
        private AppSettings()
        {
            // Initialize logger first, but handle case where it might not be available yet
            try
            {
                logger = Logger.Instance;
            }
            catch
            {
                // Logger might not be initialized yet during startup
                logger = null;
            }
            
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BetterNoFences");
            settingsPath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
            
            // Apply logging settings after loading
            ApplyLoggingSettings();
        }
        
        public void LoadSettings()
        {
            try
            {
                logger?.Debug($"Loading settings from: {settingsPath}", "AppSettings");
                
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    // Create a temporary settings object without triggering constructor
                    var tempSettings = JsonConvert.DeserializeObject<TempSettings>(json);
                    if (tempSettings != null)
                    {
                        AutoSave = tempSettings.AutoSave;
                        AutoSaveInterval = tempSettings.AutoSaveInterval;
                        ShowTooltips = tempSettings.ShowTooltips;
                        EnableAnimations = tempSettings.EnableAnimations;
                        DefaultFenceWidth = tempSettings.DefaultFenceWidth;
                        DefaultFenceHeight = tempSettings.DefaultFenceHeight;
                        DefaultTransparency = tempSettings.DefaultTransparency;
                        DefaultAutoHide = tempSettings.DefaultAutoHide;
                        DefaultAutoHideDelay = tempSettings.DefaultAutoHideDelay;
                        DefaultTitleHeight = tempSettings.DefaultTitleHeight;
                        DefaultBackgroundColor = tempSettings.DefaultBackgroundColor;
                        DefaultTitleBackgroundColor = tempSettings.DefaultTitleBackgroundColor;
                        DefaultTextColor = tempSettings.DefaultTextColor;
                        DefaultBorderColor = tempSettings.DefaultBorderColor;
                        DefaultBorderWidth = tempSettings.DefaultBorderWidth;
                        DefaultCornerRadius = tempSettings.DefaultCornerRadius;
                        DefaultShowShadow = tempSettings.DefaultShowShadow;
                        DefaultIconSize = tempSettings.DefaultIconSize;
                        DefaultItemSpacing = tempSettings.DefaultItemSpacing;
                        DefaultBackgroundTransparency = tempSettings.DefaultBackgroundTransparency;
                        DefaultTitleBackgroundTransparency = tempSettings.DefaultTitleBackgroundTransparency;
                        DefaultTextTransparency = tempSettings.DefaultTextTransparency;
                        DefaultBorderTransparency = tempSettings.DefaultBorderTransparency;
                        LogLevel = tempSettings.LogLevel ?? "Info";
                        EnableFileLogging = tempSettings.EnableFileLogging;
                        ToggleTransparencyShortcut = tempSettings.ToggleTransparencyShortcut ?? "Ctrl+Alt+T";
                        ToggleAutoHideShortcut = tempSettings.ToggleAutoHideShortcut ?? "Ctrl+Alt+H";
                        ShowAllFencesShortcut = tempSettings.ShowAllFencesShortcut ?? "Ctrl+Alt+S";
                        CreateNewFenceShortcut = tempSettings.CreateNewFenceShortcut ?? "Ctrl+Alt+N";
                        OpenSettingsShortcut = tempSettings.OpenSettingsShortcut ?? "Ctrl+Alt+O";
                        ToggleLockShortcut = tempSettings.ToggleLockShortcut ?? "Ctrl+Alt+L";
                        MinimizeAllFencesShortcut = tempSettings.MinimizeAllFencesShortcut ?? "Ctrl+Alt+M";
                        RefreshFencesShortcut = tempSettings.RefreshFencesShortcut ?? "F5";
                        
                        logger?.Info("Application settings loaded successfully", "AppSettings");
                    }
                }
                else
                {
                    logger?.Info("No existing settings file found, using defaults", "AppSettings");
                }
            }
            catch (Exception ex)
            {
                logger?.Error("Failed to load settings, using defaults", "AppSettings", ex);
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                logger?.Debug($"Saving settings to: {settingsPath}", "AppSettings");
                
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
                File.WriteAllText(settingsPath, json);
                
                // Apply logging settings after saving
                ApplyLoggingSettings();
                
                logger?.Info("Application settings saved successfully", "AppSettings");
            }
            catch (Exception ex)
            {
                logger?.Error("Failed to save settings", "AppSettings", ex);
            }
        }
        
        private void ApplyLoggingSettings()
        {
            try
            {
                if (logger != null)
                {
                    // Parse and set log level
                    if (Enum.TryParse<LogLevel>(LogLevel, out var logLevel))
                    {
                        logger.MinimumLogLevel = logLevel;
                    }
                    
                    logger.EnableFileOutput = EnableFileLogging;
                    
                    logger.Debug($"Logging settings applied - Level: {LogLevel}, File: {EnableFileLogging}", "AppSettings");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($@"Failed to apply logging settings: {e}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // Helper class for deserialization that doesn't have a constructor
        private class TempSettings
        {
            public bool AutoSave { get; set; }
            public int AutoSaveInterval { get; set; }
            public bool ShowTooltips { get; set; }
            public bool EnableAnimations { get; set; }
            public int DefaultFenceWidth { get; set; }
            public int DefaultFenceHeight { get; set; }
            public int DefaultTransparency { get; set; }
            public bool DefaultAutoHide { get; set; }
            public int DefaultAutoHideDelay { get; set; }
            public int DefaultTitleHeight { get; set; }
            public int DefaultBackgroundColor { get; set; }
            public int DefaultTitleBackgroundColor { get; set; }
            public int DefaultTextColor { get; set; }
            public int DefaultBorderColor { get; set; }
            public int DefaultBorderWidth { get; set; }
            public int DefaultCornerRadius { get; set; }
            public bool DefaultShowShadow { get; set; }
            public int DefaultIconSize { get; set; }
            public int DefaultItemSpacing { get; set; }
            public int DefaultBackgroundTransparency { get; set; }
            public int DefaultTitleBackgroundTransparency { get; set; }
            public int DefaultTextTransparency { get; set; }
            public int DefaultBorderTransparency { get; set; }
            public string LogLevel { get; set; }
            public bool EnableFileLogging { get; set; } = true;
            public string ToggleTransparencyShortcut { get; set; }
            public string ToggleAutoHideShortcut { get; set; }
            public string ShowAllFencesShortcut { get; set; }
            public string CreateNewFenceShortcut { get; set; }
            public string OpenSettingsShortcut { get; set; }
            public string ToggleLockShortcut { get; set; }
            public string MinimizeAllFencesShortcut { get; set; }
            public string RefreshFencesShortcut { get; set; }
        }
    }
}