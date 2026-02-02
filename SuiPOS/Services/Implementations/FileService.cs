using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SuiPOS.Services.Interfaces;
using SuiPOS.Settings;

namespace SuiPOS.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public FileService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string?> UploadImageAsync(IFormFile file, string folderName = "products")
        {
            if (file == null || file.Length == 0)
                return null;

            if (!IsValidImage(file))
                return null;

            try
            {
                var fileName = $"{folderName}/{Guid.NewGuid()}";

                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        PublicId = fileName,
                        Folder = "SuiPOS",
                        Overwrite = true,
                        Transformation = new Transformation()
                            .Width(1000)
                            .Height(1000)
                            .Crop("limit")
                            .Quality("auto")
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    // DEBUG: Log k?t qu?
                    Console.WriteLine($"[Cloudinary] Status: {uploadResult.StatusCode}");
                    Console.WriteLine($"[Cloudinary] URL: {uploadResult.SecureUrl}");
                    Console.WriteLine($"[Cloudinary] Error: {uploadResult.Error?.Message}");

                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return uploadResult.SecureUrl.ToString();
                    }

                    // N?u upload th?t b?i, throw exception
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message ?? "Unknown error"}");
                }
            }
            catch (Exception ex)
            {
                // Log l?i chi ti?t
                Console.WriteLine($"[Cloudinary] Exception: {ex.Message}");
                throw; // Re-throw ?? controller b?t ???c
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');

                var uploadIndex = Array.IndexOf(segments, "upload");
                if (uploadIndex >= 0 && uploadIndex + 2 < segments.Length)
                {
                    var publicIdParts = segments.Skip(uploadIndex + 2).ToArray();
                    var publicId = string.Join("/", publicIdParts);

                    publicId = Path.GetFileNameWithoutExtension(publicId);

                    if (publicIdParts.Length > 1)
                    {
                        publicId = string.Join("/", publicIdParts.Take(publicIdParts.Length - 1)) + "/" + Path.GetFileNameWithoutExtension(publicIdParts.Last());
                    }

                    var deletionParams = new DeletionParams(publicId);
                    var result = await _cloudinary.DestroyAsync(deletionParams);

                    return result.StatusCode == System.Net.HttpStatusCode.OK;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            if (!file.ContentType.StartsWith("image/"))
                return false;

            return true;
        }
    }
}

