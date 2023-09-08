using System.Drawing;
using backend.kapace.BLL.Enums;
using Microsoft.AspNetCore.Http;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IImageService
{
    Task InsertImageAsync(IFormFile imageFile,
        long? contentId,
        long historyId,
        StaticFileTypes fileType,
        CancellationToken token);
    
    Task<Image> GetAvatarByIdAsync(long imageId, long contentId, CancellationToken token);
}