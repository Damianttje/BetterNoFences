using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.IO;
using Fenceless.Win32;
using Fenceless.Util;

namespace Fenceless.Model
{
    public class FenceEntry
    {
        public string Path { get; }

        public EntryType Type { get; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        private FenceEntry(string path, EntryType type)
        {
            Path = path;
            Type = type;
        }

        public static FenceEntry FromPath(string path)
        {
            if (File.Exists(path))
                return new FenceEntry(path, EntryType.File);
            else if (Directory.Exists(path))
                return new FenceEntry(path, EntryType.Folder);
            else return null;
        }

        public Icon ExtractIcon(ThumbnailProvider thumbnailProvider)
        {
            if (Type == EntryType.File)
            {
                if (thumbnailProvider.IsSupported(Path))
                    return thumbnailProvider.GenerateThumbnail(Path);
                else
                    return Icon.ExtractAssociatedIcon(Path);
            }
            else
            {
                return IconUtil.FolderLarge;
            }
        }

        public void Open()
        {
            Task.Run(() =>
            {
                var logger = Logger.Instance;
                try
                {
                    // Verify the path still exists before trying to open
                    if (!File.Exists(Path) && !Directory.Exists(Path))
                    {
                        logger?.Warning($"Cannot open item that no longer exists: {Path}", "FenceEntry");
                        return;
                    }
                    
                    if (Type == EntryType.File)
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = Path,
                            UseShellExecute = true,
                            ErrorDialog = true
                        };
                        Process.Start(startInfo);
                        logger?.Debug($"Opened file: {Path}", "FenceEntry");
                    }
                    else if (Type == EntryType.Folder)
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{Path}\"",
                            UseShellExecute = true,
                            ErrorDialog = true
                        };
                        Process.Start(startInfo);
                        logger?.Debug($"Opened folder: {Path}", "FenceEntry");
                    }
                }
                catch (Exception e)
                {
                    logger?.Error($"Failed to open item '{Path}': {e.Message}", "FenceEntry", e);
                    // Show a user-friendly error message
                    System.Windows.Forms.MessageBox.Show(
                        $"Failed to open '{System.IO.Path.GetFileName(Path)}':\n\n{e.Message}",
                        "Error Opening Item",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error
                    );
                }
            });
        }
    }
}
