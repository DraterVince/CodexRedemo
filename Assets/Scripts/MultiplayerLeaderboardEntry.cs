using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

/// <summary>
/// Represents a leaderboard entry stored in Supabase
/// Table name: multiplayer_leaderboard
/// </summary>
[Table("multiplayer_leaderboard")]
public class MultiplayerLeaderboardEntry : BaseModel
{
    [PrimaryKey("id", false)]
    public string id { get; set; }

    [Column("username")]
    public string username { get; set; }

    [Column("user_id")]
    public string user_id { get; set; }

    [Column("levels_beaten")]
    public int levels_beaten { get; set; }

    [Column("updated_at")]
    public string updated_at { get; set; }

    public MultiplayerLeaderboardEntry()
    {
        id = System.Guid.NewGuid().ToString();
        updated_at = System.DateTime.UtcNow.ToString("o");
    }
}

