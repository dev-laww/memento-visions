using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace Game.Utils.Json.Models;

public class Save
{
    [JsonProperty("player_position")] public Vector2 PlayerPosition = new();
    [JsonProperty("player_direction")] public Vector2 PlayerDirection = new();

    [JsonProperty("player_max_health")] public float PlayerMaxHealth;
    [JsonProperty("player_health")] public float PlayerHealth;
    [JsonProperty("player_level")] public float PlayerLevel;
    [JsonProperty("player_speed")] public float PlayerSpeed;
    [JsonProperty("player_experience")] public float PlayerExperience;
    [JsonProperty("player_defense")] public float PlayerDefense;
    [JsonProperty("player_damage")] public float PlayerDamage;
    [JsonProperty("player_inventory")] public List<string> PlayerInventory = [];
    [JsonProperty("player_equipped")] public string PlayerEquipped;
}