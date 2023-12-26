using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SubtitleEditor.Core.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubtitleEditor.Database.Entities.Base;

public abstract class EntityWithIdBase : IWithId<Guid>
{
    [Key]
    [Column("_id", TypeName = "CHAR(36)", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual Guid Id { get; set; } = Guid.NewGuid();

    public virtual object GetId()
    {
        return Id;
    }

    public virtual bool HasSameId(object id)
    {
        return (Guid)id == Id;
    }

    public virtual Guid GetGenericId()
    {
        return Id;
    }

    public virtual bool HasSameId(Guid id)
    {
        return id == Id;
    }
}
