using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models.HistoryChanges;
using DataModels = backend.kapace.DAL.Models;
using Newtonsoft.Json;

namespace backend.kapace.BLL.Extensions;

internal static class JsonChangeExtension
{
    internal static HistoryUnit.JsonChanges? DeserializeToJsonChanges(this DataModels.HistoryUnit unit)
    {
        if (unit.HistoryType == HistoryType.Content)
        {
            return JsonConvert.DeserializeObject<HistoryUnit.JsonContentChanges>(unit.Text);
        }

        return JsonConvert.DeserializeObject<HistoryUnit.JsonEpisodeChanges>(unit.Text);
    }
}