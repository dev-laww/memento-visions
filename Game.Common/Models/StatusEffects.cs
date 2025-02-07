using Newtonsoft.Json;

namespace Game.Common.Models;

public class StatusEffect
{
    [JsonProperty("id")] public string Id = string.Empty;
    [JsonProperty("remaining_duration")] public float RemainingDuration;
}