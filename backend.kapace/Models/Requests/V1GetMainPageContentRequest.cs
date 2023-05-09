using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Requests;

public record V1GetMainPageContentRequest(
    MainPageType[] MainPageTypes,
    int Limit,
    int Offset);