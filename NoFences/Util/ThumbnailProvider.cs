using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NoFences.Util
{
    public class ThumbnailProvider : IDisposable
    {
        // Supported .NET images as per https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image.fromfile
        private static readonly string[] SupportedExtensions =
        {
            ".bmp",
            ".gif",
            ".jpg",
            ".jpeg",
            ".png",
            ".tiff",
            ".tif"
        };

        private class ThumbnailState
        {
            public Icon icon;
        }

        // Only allow 4 concurrent images to be decoded to try and prevent OOM errors
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(4);
        private readonly IDictionary<string, ThumbnailState> iconCache = new Dictionary<string, ThumbnailState>();
        public event EventHandler IconThumbnailLoaded;
        private bool disposed = false;

        public bool IsSupported(string path)
        {
            return SupportedExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        public Icon GenerateThumbnail(string path)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ThumbnailProvider));

            if (!iconCache.ContainsKey(path))
            {
                return SubmitGeneratorTask(path).icon;
            }
            else
            {
                return iconCache[path].icon;
            }
        }

        private ThumbnailState SubmitGeneratorTask(string path)
        {
            var state = new ThumbnailState() { icon = Icon.ExtractAssociatedIcon(path) };
            iconCache[path] = state;

            Task.Run(() =>
            {
                if (disposed) return;
                
                try
                {
                    semaphore.Wait();
                    if (disposed) return;

                    using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
                    {
                        using (var img = Image.FromStream(ms))
                        {
                            var thumb = (Bitmap)img.GetThumbnailImage(32, 32, () => false, IntPtr.Zero);
                            var icon = Icon.FromHandle(thumb.GetHicon());
                            state.icon = icon;
                            IconThumbnailLoaded?.Invoke(this, new EventArgs());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to generate thumbnail for {path}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
            });
            return state;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                semaphore?.Dispose();
                
                foreach (var cache in iconCache.Values)
                {
                    cache.icon?.Dispose();
                }
                iconCache.Clear();
            }
        }
    }
}
