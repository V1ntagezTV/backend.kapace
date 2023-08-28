using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace backend.kapace.Models.Requests;

public sealed record InsertContentImageRequest(
    [Required] long ContentId, 
    [Required] long HistoryId,
    [Required] IFormFile Image);