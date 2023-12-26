using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SubtitleEditor.Database;

public abstract class DbContextBase<TContext> : DbContext where TContext : DbContext
{
    public DbContextBase(DbContextOptions<TContext> options) : base(options) { }

    public void DetachAllEntities()
    {
        foreach (var entity in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            )
        {
            Entry(entity.Entity).State = EntityState.Detached;
        }
    }

    public void Detach<TEntity>(TEntity entity)
    {
        if (entity != null)
        {
            Entry(entity).State = EntityState.Detached;
        }
    }

    public void Detach<TEntity>(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities.Where(e => e != null))
        {
            Entry(entity!).State = EntityState.Detached;
        }
    }

    public int ExecuteSqlRawQuietly(string sql, params object[] parameters)
    {
        try
        {
            return Database.ExecuteSqlRaw(sql, parameters);
        }
        catch
        {
            return 0;
        }
    }
}
