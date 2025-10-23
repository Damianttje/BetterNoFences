using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fenceless.Util
{
    /// <summary>
    /// Utility class for validating and sanitizing file paths to prevent security issues
    /// </summary>
    public static class PathValidator
    {
        private static readonly string[] _dangerousPatterns = {
            "..",
            "~",
            "$",
            "%",
            "&",
            "|",
            "\"",
            "'",
            "`",
            ";",
            "<",
            ">",
            "{",
            "}",
            "[",
            "]"
        };

        private static readonly string[] _forbiddenExtensions = {
            ".exe",
            ".bat",
            ".cmd",
            ".com",
            ".pif",
            ".scr",
            ".vbs",
            ".js",
            ".jar",
            ".app",
            ".deb",
            ".rpm",
            ".dmg",
            ".pkg",
            ".msi"
        };

        private static readonly Regex _validPathRegex = new Regex(
            @"^[a-zA-Z]:\\([^\\:*?""<>|]+\\)*[^\\:*?""<>|]*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Validates if a path is safe to use
        /// </summary>
        public static bool IsPathSafe(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // Get full path to resolve relative paths
                var fullPath = Path.GetFullPath(path);
                
                // Check for null bytes injection
                if (fullPath.Contains('\0'))
                    return false;

                // Check for dangerous patterns
                if (_dangerousPatterns.Any(pattern => fullPath.Contains(pattern)))
                    return false;

                // Check for extremely long paths
                if (fullPath.Length > 32767) // Windows MAX_PATH
                    return false;

                // Basic format validation for Windows paths
                if (!_validPathRegex.IsMatch(fullPath))
                    return false;

                // Check if path points to system directories (optional)
                if (IsSystemPath(fullPath))
                {
                    Logger.Instance?.Warning($"Attempted to access system path: {fullPath}", "PathValidator");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance?.Warning($"Path validation failed for '{path}': {ex.Message}", "PathValidator");
                return false;
            }
        }

        /// <summary>
        /// Sanitizes a path by removing dangerous characters
        /// </summary>
        public static string SanitizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            try
            {
                var fullPath = Path.GetFullPath(path);
                
                // Remove null bytes
                fullPath = fullPath.Replace("\0", string.Empty);
                
                // Remove dangerous characters
                foreach (var pattern in _dangerousPatterns)
                {
                    fullPath = fullPath.Replace(pattern, string.Empty);
                }

                // Limit path length
                if (fullPath.Length > 32767)
                {
                    fullPath = fullPath.Substring(0, 32767);
                }

                return fullPath;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates if a file extension is safe to execute
        /// </summary>
        public static bool IsExecutableExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            return _forbiddenExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Checks if a path is a system-critical directory
        /// </summary>
        private static bool IsSystemPath(string path)
        {
            try
            {
                var systemPaths = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    Environment.GetFolderPath(Environment.SpecialFolder.System),
                    Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };

                return systemPaths.Any(systemPath => 
                    path.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a path is within allowed bounds
        /// </summary>
        public static bool IsPathWithinAllowedBounds(string path, string allowedRoot = null)
        {
            if (!IsPathSafe(path))
                return false;

            try
            {
                var fullPath = Path.GetFullPath(path);

                // If no allowed root specified, allow user profile and desktop
                if (string.IsNullOrEmpty(allowedRoot))
                {
                    var userPaths = new[]
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
                    };

                    return userPaths.Any(userPath => 
                        fullPath.StartsWith(userPath, StringComparison.OrdinalIgnoreCase));
                }

                var allowedFullPath = Path.GetFullPath(allowedRoot);
                return fullPath.StartsWith(allowedFullPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a safe relative path from an absolute path
        /// </summary>
        public static string GetSafeRelativePath(string fullPath, string basePath)
        {
            if (!IsPathSafe(fullPath) || !IsPathSafe(basePath))
                return string.Empty;

            try
            {
                return Path.GetRelativePath(basePath, fullPath);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Validates file name without path
        /// </summary>
        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Check for invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.IndexOfAny(invalidChars) >= 0)
                return false;

            // Check for reserved names
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", 
                                     "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                                     "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
            if (reservedNames.Contains(nameWithoutExt))
                return false;

            return true;
        }
    }
}