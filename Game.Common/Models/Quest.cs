using Newtonsoft.Json;

namespace Game.Common.Models;

public class Quest
{
    [JsonProperty("id")] public string Id = string.Empty;
    [JsonProperty("completed")] public bool Completed;
}