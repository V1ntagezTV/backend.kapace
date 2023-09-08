using System.Drawing;
using System.Drawing.Imaging;
using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository;
using Microsoft.AspNetCore.Http;

namespace backend.kapace.BLL.Services;

public class ImageService : IImageService
{
    private readonly IStaticFilesRepository _staticFilesRepository;
    private readonly IChangesHistoryService _changesHistoryService;

    private static readonly string StaticFilesPath =
        Path.Combine(ProjectDirectoryHelper.GetProjectDirectory(), "StaticFiles");
 
    
    public ImageService(
        IStaticFilesRepository staticFilesRepository,
        IChangesHistoryService changesHistoryService)
    {
        _staticFilesRepository = staticFilesRepository;
        _changesHistoryService = changesHistoryService;
    }
    
    public async Task InsertImageAsync(
        IFormFile imageFile,
        long? contentId,
        long historyId,
        StaticFileTypes fileType,
        CancellationToken token)
    {
        // Если загружается картинка для только созданного контента, то айди будет HistoryId.
        var targetContentId = contentId ?? historyId;
        
        var folderPath = Path.Combine(StaticFilesPath, $"{fileType}", $"{targetContentId}");
        if (imageFile.Length <= 0)
        {
            return;
        }
        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        if(imageFile.ContentType.Contains("image"))
        {
            var imageId = await _staticFilesRepository.InsertAsync(
                imageFile.FileName,
                targetContentId,
                StaticFileLinkType.Content,
                createdAt: DateTimeOffset.UtcNow,
                token);

            await _changesHistoryService.UpdateImageAsync(historyId, imageId, token);

            using var img = Image.FromStream(imageFile.OpenReadStream());
            var imagePath = Path.Combine(folderPath, imageId + ".png");
            await using Stream fileStream = new FileStream(imagePath, FileMode.Create);
            img.Save(fileStream, ImageFormat.Png);
        }
        else
        {
            throw new ArgumentException($"File must be one of the img types.");
        }
    }

    public async Task<Image> GetAvatarByIdAsync(long imageId, long contentId, CancellationToken token)
    { 
        var imagePath = Path.Combine(
            StaticFilesPath,
            $"{StaticFileLinkType.Content}",
            $"{contentId}",
            $"{imageId}.png"
        );
        
        var image = await Task.Run(() => Image.FromFile(imagePath), token).ConfigureAwait(false);

        return image;
    }
}