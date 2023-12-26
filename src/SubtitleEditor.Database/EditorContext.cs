using Microsoft.EntityFrameworkCore;
using SubtitleEditor.Core.Contexts;
using SubtitleEditor.Core.Helpers;
using SubtitleEditor.Database.Entities;

namespace SubtitleEditor.Database;

public class EditorContext : DbContextBase<EditorContext>
{
    private static bool _ensureUpdated = false;

    public EditorContext(DbContextOptions<EditorContext> options) : base(options)
    {
        // 檢查資料庫是否存在，如果不存在則建立。
        if (!Database.EnsureCreated() && !_ensureUpdated)
        {
            _ensureUpdated = true;

            // 檢查是否有新的資料表需要建立。如果走到這，代表現有的資料庫已存在，所以需要確保所有資料表都正確。
            // --------------------------
            // 1.0.24 新增 FixBooks
            Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS \"FixBooks\" (" +
                "\"_id\" CHAR(36) NOT NULL UNIQUE, \"_model\" NVARCHAR(256), " +
                "\"_original\" NVARCHAR(256) NOT NULL, \"_correction\" NVARCHAR(256) NOT NULL, " +
                "\"_create\" TEXT NOT NULL, CONSTRAINT \"PK_FixBooks\" PRIMARY KEY(\"_id\"));"
                );

            // 1.0.25 在 Topics 資料表增加 _asr_task 欄位。
            ExecuteSqlRawQuietly("ALTER TABLE 'Topics' ADD COLUMN '_asr_task' TEXT; " +
                "ALTER TABLE 'Media' ADD COLUMN '_asr_process_start' TEXT; ALTER TABLE 'Media' ADD COLUMN '_asr_process_end' TEXT; ALTER TABLE 'Media' ADD COLUMN '_process_end' TEXT;");

            // 1.0.28 在 SystemOption 資料表增加 _type 欄位。
            ExecuteSqlRawQuietly("ALTER TABLE 'SystemOptions' ADD COLUMN '_type' NVATCHAR(256);");

            // 1.0.34 在 Media 資料表增加 _asr_status 與 _convert_status 欄位。
            ExecuteSqlRawQuietly("ALTER TABLE 'Media' ADD COLUMN '_asr_status' TEXT NOT NULL DEFAULT 'ASRWaiting'; ALTER TABLE 'Media' ADD COLUMN '_convert_status' TEXT NOT NULL DEFAULT 'FFMpegWaiting';" +
                "update Media set _asr_status = 'ASRWaiting', _convert_status = 'FFMpegWaiting' where _status = 'ASRWaiting'; " + 
                "update Media set _asr_status = 'ASRProcessing', _convert_status = 'FFMpegWaiting' where _status = 'ASRProcessing'; " + 
                "update Media set _asr_status = 'ASRCanceled', _convert_status = 'FFMpegWaiting' where _status = 'ASRCanceled'; " + 
                "update Media set _asr_status = 'ASRFailed', _convert_status = 'FFMpegWaiting' where _status = 'ASRFailed'; " + 
                "update Media set _convert_status = 'FFMpegWaiting' where _status = 'FFMpegWaiting'; " + 
                "update Media set _convert_status = 'FFMpegProcessing' where _status = 'FFMpegProcessing'; " + 
                "update Media set _convert_status = 'FFMpegCompleted' where _status = 'Completed'; " + 
                "update Media set _convert_status = 'FFMpegFailed' where _status = 'Failed'; " + 
                "update Media set _asr_status = 'ASRCompleted' where (_status = 'FFMpegWaiting' or _status = 'FFMpegProcessing' or _status = 'Completed' or _status = 'Failed') and _asr_process_end is not null; " + 
                "update Media set _asr_status = 'ASRSkipped' where (_status = 'FFMpegWaiting' or _status = 'FFMpegProcessing' or _status = 'Completed' or _status = 'Failed') and _asr_process_start is null;");

            // 1.0.37 在 Subtitle 增加了 _word_limit 欄位。
            ExecuteSqlRawQuietly("ALTER TABLE 'Subtitles' ADD COLUMN '_word_limit' INTEGER;");

            //1.0.38 在Subtitle 增加了 _created_option 欄位。
            ExecuteSqlRawQuietly("ALTER TABLE 'Subtitles' ADD COLUMN '_created_option' NVARCHAR(10);");
        }
    }

    public virtual DbSet<Log> Logs => Set<Log>();
    public virtual DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<UserSecret> UserSecrets => Set<UserSecret>();
    public virtual DbSet<UserLogin> UserLogins => Set<UserLogin>();
    public virtual DbSet<UserMeta> UserMetas => Set<UserMeta>();
    public virtual DbSet<SystemOption> SystemOptions => Set<SystemOption>();
    public virtual DbSet<UserGroupLink> UserGroupLinks => Set<UserGroupLink>();

    public virtual DbSet<Topic> Topics => Set<Topic>();
    public virtual DbSet<Media> Media => Set<Media>();
    public virtual DbSet<Subtitle> Subtitles => Set<Subtitle>();
    public virtual DbSet<FixBook> FixBooks => Set<FixBook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Log>(log =>
        {
            log.HasKey(e => e.Id);
            log.HasIndex(e => e.ActionId);
            log.HasIndex(e => e.IPAddress);
            log.HasIndex(e => e.UserId);
            log.HasIndex(e => e.Time);
            log.HasIndex(e => new { e.Time, e.ActionText });
            log.HasIndex(e => new { e.Time, e.Target });
            log.HasIndex(e => new { e.Time, e.ActionText, e.Target });
        });

        modelBuilder.Entity<UserGroup>(userGroup =>
        {
            userGroup
                .Property(e => e.GroupType)
                .HasConversion(
                    o => o.HasValue ? o.Value.ToString() : null,
                    o => !string.IsNullOrWhiteSpace(o) ? PermissionType.SystemAdmin.Parse(o) : null
                    );
        });

        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(e => e.Id);
            user.HasIndex(e => e.Account);
            user.HasIndex(e => new { e.Account, e.Status });
            user.Property(e => e.Status)
                .HasConversion(
                    o => o.ToString(),
                    o => UserStatus.Enabled.Parse(o)
                    );
        });

        modelBuilder.Entity<UserSecret>(secret =>
        {
            secret.HasKey(e => e.Id);
            secret.HasIndex(e => e.Create);
            secret.HasOne(e => e.User).WithMany(e => e.Secrets).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            secret.Property(e => e.Reason)
                .HasConversion(
                    o => o.ToString(),
                    o => UserSecretCreateReason.Other.Parse(o)
                    );
        });

        modelBuilder.Entity<UserLogin>(login =>
        {
            login.HasKey(e => e.Id);
            login.HasOne(e => e.User).WithMany(e => e.Logins).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserMeta>(meta =>
        {
            meta.HasKey(e => new { e.Key, e.UserId });
            meta.HasOne(e => e.User).WithMany(e => e.Metas).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserGroupLink>(link =>
        {
            link.HasKey(e => new { e.UserId, e.UserGroupId });
            link.HasOne(l => l.User).WithMany(e => e.UserGroupLinks).HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
            link.HasOne(l => l.UserGroup).WithMany(e => e.UserGroupLinks).HasForeignKey(l => l.UserGroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SystemOption>(systemOption =>
        {
            systemOption.HasKey(e => new { e.Name, e.Create });
        });

        modelBuilder.Entity<Topic>(topic =>
        {
            topic.HasKey(e => e.Id);
            topic.HasOne(e => e.Media).WithOne(e => e.Topic).HasForeignKey<Media>(e => e.TopicId).OnDelete(DeleteBehavior.Cascade);
            topic.HasOne(e => e.Subtitle).WithOne(e => e.Topic).HasForeignKey<Subtitle>(e => e.TopicId).OnDelete(DeleteBehavior.Cascade);
            topic.Property(e => e.Status)
                .HasConversion(
                    o => o.ToString(),
                    o => TopicStatus.Normal.Parse(o)
                    );
        });

        modelBuilder.Entity<Media>(media =>
        {
            media.HasKey(e => e.TopicId);
            //media.Property(e => e.Status)
            //    .HasConversion(
            //        o => o.ToString(),
            //        o => StreamMediaStatus.ASRWaiting.Parse(o)
            //        );
            media.Property(e => e.AsrStatus)
                .HasConversion(
                    o => o.ToString(),
                    o => AsrMediaStatus.ASRWaiting.Parse(o)
                    );
            media.Property(e => e.ConvertStatus)
                .HasConversion(
                    o => o.ToString(),
                    o => ConvertMediaStatus.FFMpegWaiting.Parse(o)
                    );
        });

        modelBuilder.Entity<Subtitle>(subtitle =>
        {
            subtitle.HasKey(e => e.TopicId);
        });
    }
}
