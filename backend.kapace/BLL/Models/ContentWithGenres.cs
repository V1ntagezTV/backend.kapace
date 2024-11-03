using backend.kapace.DAL.Models;
using Content = backend.kapace.BLL.Models.VideoService.Content;

namespace backend.kapace.BLL.Models;

public record ContentWithGenres(Content ContentInfo, Genre.Genre[] Genres);