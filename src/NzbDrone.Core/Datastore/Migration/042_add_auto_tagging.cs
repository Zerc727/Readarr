using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(042)]
    public class add_auto_tagging : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.Table("AutoTagging")
                  .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Specifications").AsString().NotNullable().WithDefaultValue("[]")
                  .WithColumn("RemoveTags").AsBoolean().NotNullable().WithDefaultValue(false)
                  .WithColumn("Tags").AsString().NotNullable().WithDefaultValue("[]");
        }
    }
}
