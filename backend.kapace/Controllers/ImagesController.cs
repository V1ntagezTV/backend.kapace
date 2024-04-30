using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[Route("image")]
public class ImagesController : Controller
{
    private const string PngContentType = "image/png";
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    [HttpPost("content/insert")]
    public async Task InsertContentImage(
        [FromForm] InsertContentImageRequest request,
        CancellationToken token)
    {
        if (request?.Image is null)
        {
            throw new ArgumentNullException($"{nameof(request.Image)} must be initialized");
        }
        
        await _imageService.InsertImageAsync(
            request.Image,
            request.ContentId,
            request.HistoryId,
            StaticFileTypes.Content,
            token
        );
    }

    [HttpGet("content/get-avatar")]
    public async Task<ActionResult> GetAvatarByImageId(
        GetByContentImageIdRequest request,
        CancellationToken token)
    {
        var image = await _imageService.GetAvatarByIdAsync(request.ImageId, request.ContentId, token);

        return File(image, PngContentType);
    }
}