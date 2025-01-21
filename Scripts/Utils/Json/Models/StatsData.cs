using Newtonsoft.Json;

namespace Game.Utils.Json.Models;

public class StatsData
{
    [JsonProperty("max_health")] public float MaxHealth;
    [JsonProperty("health")] public float Health;
    [JsonProperty("level")] public float Level;
    [JsonProperty("speed")] public float Speed;
    [JsonProperty("experience")] public float Experience;
    [JsonProperty("defense")] public float Defense;
    [JsonProperty("damage")] public float Damage;
}