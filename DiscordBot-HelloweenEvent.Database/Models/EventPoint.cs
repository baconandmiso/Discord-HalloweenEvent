using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot_HelloweenEvent.Database.Models;

[Table("points", Schema = "event")]
public class EventPoint
{
    [Key]
    [Column("user_id")]
    public ulong UserId { get; set; }

    [Column("score")]
    public int Score { get; set; }
}
