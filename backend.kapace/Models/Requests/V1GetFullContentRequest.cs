using backend.kapace.BLL.Enums;
using backend.kapace.Controllers;

namespace backend.kapace.Models.Requests;

public record V1GetFullContentRequest(
    long ContentId, 
    ContentSelectedInfo SelectedInfo,
    long? UserId);