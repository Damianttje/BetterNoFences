using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Fenceless.Util;

namespace Fenceless.Model
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
        
        // Startup settings
        public bool StartWithWindows { get; set; } = false;

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
        
        [JsonIgnore]
        private static readonly ReaderWriterLockSlim _settingsLock = new ReaderWriterLockSlim();
        
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
            
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Fenceless");
            settingsPath = Path.Combine(appDataPath, "settings.json");
            LoadSettings();
            
            // Apply logging settings after loading
            ApplyLoggingSettings();
        }
        
        public void LoadSettings()
        {
            _settingsLock.EnterWriteLock();
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
                        // Validate and apply settings with bounds checking
                        AutoSave = tempSettings.AutoSave;
                        AutoSaveInterval = Math.Max(5, Math.Min(3600, tempSettings.AutoSaveInterval)); // 5 seconds to 1 hour
                        ShowTooltips = tempSettings.ShowTooltips;
                        EnableAnimations = tempSettings.EnableAnimations;
                        DefaultFenceWidth = Math.Max(200, Math.Min(2000, tempSettings.DefaultFenceWidth)); // 200-2000 pixels
                        DefaultFenceHeight = Math.Max(200, Math.Min(2000, tempSettings.DefaultFenceHeight)); // 200-2000 pixels
                        DefaultTransparency = Math.Max(0, Math.Min(100, tempSettings.DefaultTransparency)); // 0-100%
                        DefaultAutoHide = tempSettings.DefaultAutoHide;
                        DefaultAutoHideDelay = Math.Max(500, Math.Min(10000, tempSettings.DefaultAutoHideDelay)); // 0.5-10 seconds
                        DefaultTitleHeight = Math.Max(15, Math.Min(50, tempSettings.DefaultTitleHeight)); // 15-50 pixels
                        DefaultBackgroundColor = tempSettings.DefaultBackgroundColor;
                        DefaultTitleBackgroundColor = tempSettings.DefaultTitleBackgroundColor;
                        DefaultTextColor = tempSettings.DefaultTextColor;
                        DefaultBorderColor = tempSettings.DefaultBorderColor;
                        DefaultBorderWidth = Math.Max(0, Math.Min(10, tempSettings.DefaultBorderWidth)); // 0-10 pixels
                        DefaultCornerRadius = Math.Max(0, Math.Min(50, tempSettings.DefaultCornerRadius)); // 0-50 pixels
                        DefaultShowShadow = tempSettings.DefaultShowShadow;
                        DefaultIconSize = Math.Max(16, Math.Min(256, tempSettings.DefaultIconSize)); // 16-256 pixels
                        DefaultItemSpacing = Math.Max(5, Math.Min(50, tempSettings.DefaultItemSpacing)); // 5-50 pixels
                        DefaultBackgroundTransparency = Math.Max(0, Math.Min(100, tempSettings.DefaultBackgroundTransparency)); // 0-100%
                        DefaultTitleBackgroundTransparency = Math.Max(0, Math.Min(100, tempSettings.DefaultTitleBackgroundTransparency)); // 0-100%
                        DefaultTextTransparency = Math.Max(0, Math.Min(100, tempSettings.DefaultTextTransparency)); // 0-100%
                        DefaultBorderTransparency = Math.Max(0, Math.Min(100, tempSettings.DefaultBorderTransparency)); // 0-100%
                        LogLevel = ValidateLogLevel(tempSettings.LogLevel);
                        EnableFileLogging = tempSettings.EnableFileLogging;
                        ToggleTransparencyShortcut = ValidateShortcut(tempSettings.ToggleTransparencyShortcut) ?? "Ctrl+Alt+T";
                        ToggleAutoHideShortcut = ValidateShortcut(tempSettings.ToggleAutoHideShortcut) ?? "Ctrl+Alt+H";
                        ShowAllFencesShortcut = ValidateShortcut(tempSettings.ShowAllFencesShortcut) ?? "Ctrl+Alt+S";
                        CreateNewFenceShortcut = ValidateShortcut(tempSettings.CreateNewFenceShortcut) ?? "Ctrl+Alt+N";
                        OpenSettingsShortcut = ValidateShortcut(tempSettings.OpenSettingsShortcut) ?? "Ctrl+Alt+O";
                        ToggleLockShortcut = ValidateShortcut(tempSettings.ToggleLockShortcut) ?? "Ctrl+Alt+L";
                        MinimizeAllFencesShortcut = ValidateShortcut(tempSettings.MinimizeAllFencesShortcut) ?? "Ctrl+Alt+M";
                        RefreshFencesShortcut = ValidateShortcut(tempSettings.RefreshFencesShortcut) ?? "F5";
                        StartWithWindows = tempSettings.StartWithWindows;
                        
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
            finally
            {
                _settingsLock.ExitWriteLock();
            }
        }
        
        public void SaveSettings()
        {
            _settingsLock.EnterReadLock();
            try
            {
                logger?.Debug($"Saving settings to: {settingsPath}", "AppSettings");
                
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                AtomicFileWrite(settingsPath, json);
                
                // Apply logging settings after saving
                ApplyLoggingSettings();
                
                logger?.Info("Application settings saved successfully", "AppSettings");
            }
            catch (Exception ex)
            {
                logger?.Error("Failed to save settings", "AppSettings", ex);
            }
            finally
            {
                _settingsLock.ExitReadLock();
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
        
        private void AtomicFileWrite(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var tempPath = path + ".tmp";
            var backupPath = path + ".bak";
            
            try
            {
                // Write to temporary file
                File.WriteAllText(tempPath, content);
                
                // Create backup of existing file if it exists
                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);
                }
                
                // Replace original with temporary file
                File.Replace(tempPath, path, backupPath);
                
                // Clean up backup file
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch
            {
                // Clean up temporary file if it exists
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                
                // Restore from backup if it exists
                if (File.Exists(backupPath))
                {
                    try { File.Copy(backupPath, path, true); } catch { }
                }
                
                throw;
            }
        }
        
        private string ValidateLogLevel(string logLevel)
        {
            if (string.IsNullOrEmpty(logLevel))
                return "Info";
                
            var validLevels = new[] { "Debug", "Info", "Warning", "Error", "Critical" };
            return validLevels.Contains(logLevel) ? logLevel : "Info";
        }
        
        private string ValidateShortcut(string shortcut)
        {
            if (string.IsNullOrEmpty(shortcut))
                return null;
                
            // Basic validation - ensure it contains at least one key
            var parts = shortcut.Split('+');
            if (parts.Length == 0)
                return null;
                
            // Check for at least one valid key
            var validKeys = new[] { "Ctrl", "Alt", "Shift", "Windows", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
                                  "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", 
                                  "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
                                  "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
                                  
            return parts.Any(p => validKeys.Contains(p.Trim())) ? shortcut : null;
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
            public bool StartWithWindows { get; set; }
        }
    }
}