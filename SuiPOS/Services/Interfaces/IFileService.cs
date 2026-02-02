using Microsoft.AspNetCore.Http;

namespace SuiPOS.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Upload file và tr? v? URL c?a file
        /// </summary>
        Task<string?> UploadImageAsync(IFormFile file, string folderName = "products");

        /// <summary>
        /// Xóa file theo URL
        /// </summary>
        Task<bool> DeleteImageAsync(string imageUrl);

        /// <summary>
        /// Validate file ?nh
        /// </summary>
        bool IsValidImage(IFormFile file);
    }
}
