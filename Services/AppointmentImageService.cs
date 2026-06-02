using System;
using System.IO;

namespace CruzNeryClinic.Services
{
    // Stores uploaded teeth photos on disk next to the database, returning the
    // saved path so it can be recorded in the AppointmentImages table.
    public static class AppointmentImageService
    {
        private static readonly string ImageFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CruzNeryClinic",
                "AppointmentImages");

        // Copies the chosen source image into the app image folder and returns the
        // stored file path. The original file is left untouched.
        public static string SaveImage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Image file was not found.", sourcePath);

            if (!Directory.Exists(ImageFolder))
                Directory.CreateDirectory(ImageFolder);

            string extension = Path.GetExtension(sourcePath);
            string fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}";
            string destinationPath = Path.Combine(ImageFolder, fileName);

            File.Copy(sourcePath, destinationPath, overwrite: false);

            return destinationPath;
        }
    }
}
