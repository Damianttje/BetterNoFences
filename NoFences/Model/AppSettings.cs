using Newtonsoft.Json;
using System;
using System.IO;
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
        public string Theme { get; set; } = "Auto";
        public int DefaultFenceWidth { get; set; } = 300;
        public int DefaultFenceHeight { get; set; } = 300;
        
        // New transparency and autohide settings
        public int DefaultTransparency { get; set; } = 100;
        public bool DefaultAutoHide { get; set; } = false;
        public int DefaultAutoHideDelay { get; set; } = 2000;
        
        // Logging settings
        public string LogLevel { get; set; } = "Info";
        public bool EnableFileLogging { get; set; } = true;
        
        // Global keyboard shortcuts
        public string ToggleTransparencyShortcut { get; set; } = "Ctrl+Alt+T";
        public string ToggleAutoHideShortcut { get; set; } = "Ctrl+Alt+H";
        public string ShowAllFencesShortcut { get; set; } = "Ctrl+Alt+S";
        
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
                        Theme = tempSettings.Theme ?? "Auto";
                        DefaultFenceWidth = tempSettings.DefaultFenceWidth;
                        DefaultFenceHeight = tempSettings.DefaultFenceHeight;
                        DefaultTransparency = tempSettings.DefaultTransparency;
                        DefaultAutoHide = tempSettings.DefaultAutoHide;
                        DefaultAutoHideDelay = tempSettings.DefaultAutoHideDelay;
                        LogLevel = tempSettings.LogLevel ?? "Info";
                        EnableFileLogging = tempSettings.EnableFileLogging;
                        ToggleTransparencyShortcut = tempSettings.ToggleTransparencyShortcut ?? "Ctrl+Alt+T";
                        ToggleAutoHideShortcut = tempSettings.ToggleAutoHideShortcut ?? "Ctrl+Alt+H";
                        ShowAllFencesShortcut = tempSettings.ShowAllFencesShortcut ?? "Ctrl+Alt+S";
                        
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
            catch (Exception)
            {
                // Ignore errors during logging configuration
            }
        }
        
        // Helper class for deserialization that doesn't have a constructor
        private class TempSettings
        {
            public bool AutoSave { get; set; }
            public int AutoSaveInterval { get; set; }
            public bool ShowTooltips { get; set; }
            public bool EnableAnimations { get; set; }
            public string Theme { get; set; }
            public int DefaultFenceWidth { get; set; }
            public int DefaultFenceHeight { get; set; }
            public int DefaultTransparency { get; set; }
            public bool DefaultAutoHide { get; set; }
            public int DefaultAutoHideDelay { get; set; }
            public string LogLevel { get; set; }
            public bool EnableFileLogging { get; set; } = true;
            public string ToggleTransparencyShortcut { get; set; }
            public string ToggleAutoHideShortcut { get; set; }
            public string ShowAllFencesShortcut { get; set; }
        }
    }
}