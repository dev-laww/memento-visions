using Godot;
using Newtonsoft.Json;

namespace Game.Common.Models;

public class Player
{
    [JsonProperty("position")] public Vector2 Position = Vector2.Zero;
    [JsonProperty("direction")] public Vector2 Direction = Vector2.Down;
    [JsonProperty("stats")] public Stats Stats = new();
}