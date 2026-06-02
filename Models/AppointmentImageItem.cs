using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace CruzNeryClinic.Models
{
    // One uploaded teeth photo shown as a thumbnail in the appointment overlays.
    // While the appointment is being created, FilePath points at the user's
    // chosen source file; after saving it points at the stored copy.
    public class AppointmentImageItem
    {
        // 0 while the image is only staged in an add form; set once it is stored
        // in the AppointmentImages table.
        public int AppointmentImageId { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string FileName => Path.GetFileName(FilePath);

        // Decoded once at a small size so the thumbnail list stays light and the
        // underlying file is not locked.
        public BitmapImage? Thumbnail
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath))
                        return null;

                    BitmapImage image = new();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.DecodePixelWidth = 160;
                    image.UriSource = new Uri(FilePath, UriKind.Absolute);
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
