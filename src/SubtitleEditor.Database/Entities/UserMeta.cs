using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities;

public class UserMeta
{
    [Key]
    [Required]
    [Column("_key", TypeName = "varchar(128)", Order = 1)]
    [StringLength(128)]
    public string Key { get; set; } = null!;

    [Key]
    [Required]
    [Column("_user_id", TypeName = "CHAR(36)", Order = 2)]
    public Guid UserId { get; set; }

    [Column("_data", TypeName = "TEXT")]
    public string? Data { get; set; }

    [Required]
    [Column("_create")]
    public DateTime Create { get; set; } = DateTime.Now;

    [Required]
    [Column("_update")]
    public DateTime Update { get; set; } = DateTime.Now;

    public virtual User User { get; set; } = null!;
}
