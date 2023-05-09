using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Response;

public record V1GetMainPageContentResponse(V1GetMainPageContentResponse.V1GetMainPagePair[] Contents)
{
    public class V1GetMainPagePair
    {
        public required MainPageType MainPageType { get; init; }

        public required V1GetMainPageContent[] Content { get; init; }
    }
    
    public class V1GetMainPageContent
    {
        public long Id { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public decimal ImportStars { get; set; }
        public int SeriesOut { get; set; }
        public int SeriesPlanned { get; set; }
    }
}