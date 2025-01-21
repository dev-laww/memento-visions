using Godot;
using Newtonsoft.Json;

namespace Game.Utils.Json.Models;

public class PlayerData
{
    [JsonProperty("position")] public Vector2 Position = Vector2.Zero;
    [JsonProperty("direction")] public Vector2 Direction = Vector2.Down;
    [JsonProperty("stats")] public StatsData Stats = new();
}