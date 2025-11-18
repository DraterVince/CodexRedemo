using System;
using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

/// <summary>
/// Represents player data stored in Supabase
/// Table name: player_data
/// </summary>
[Table("player_data")]
public class PlayerData : BaseModel
{
    [PrimaryKey("id", false)]
    public string id { get; set; }

    [Column("user_id")]
    public string user_id { get; set; }

    [Column("email")]
    public string email { get; set; }

    [Column("username")]
    public string username { get; set; }

    [Column("levels_unlocked")]
    public int levels_unlocked { get; set; }

    [Column("current_money")]
    public int current_money { get; set; }

    [Column("unlocked_cosmetics")]
    public string unlocked_cosmetics { get; set; } // JSON string of cosmetic IDs

    [Column("created_at")]
    public string created_at { get; set; }

    [Column("updated_at")]
    public string updated_at { get; set; }

    public PlayerData()
    {
        id = System.Guid.NewGuid().ToString();
        unlocked_cosmetics = "[]";
        created_at = System.DateTime.UtcNow.ToString("o");
        updated_at = System.DateTime.UtcNow.ToString("o");
    }
}
