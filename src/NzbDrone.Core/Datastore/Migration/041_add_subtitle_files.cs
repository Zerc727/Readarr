using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(041)]
    public class add_subtitle_files : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("SubtitleFiles")
                  .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                  .WithColumn("AuthorId").AsInt32().NotNullable()
                  .WithColumn("BookFileId").AsInt32().Nullable()
                  .WithColumn("BookId").AsInt32().Nullable()
                  .WithColumn("RelativePath").AsString().NotNullable()
                  .WithColumn("Added").AsDateTime().NotNullable()
                  .WithColumn("LastUpdated").AsDateTime().NotNullable()
                  .WithColumn("Extension").AsString().NotNullable()
                  .WithColumn("Language").AsInt32().NotNullable().WithDefaultValue(0)
                  .WithColumn("LanguageTags").AsString().Nullable()
                  .WithColumn("AiTranslated").AsBoolean().NotNullable().WithDefaultValue(false)
                  .WithColumn("MachineTranslated").AsBoolean().NotNullable().WithDefaultValue(false);
        }
    }
}
