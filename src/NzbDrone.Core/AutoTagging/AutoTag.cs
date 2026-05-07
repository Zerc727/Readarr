using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.AutoTagging
{
    public class AutoTag : ModelBase
    {
        public string Name { get; set; }
        public bool RemoveTags { get; set; }
        public List<int> Tags { get; set; }
        public List<IAutoTagSpecification> Specifications { get; set; }

        public AutoTag()
        {
            Tags = new List<int>();
            Specifications = new List<IAutoTagSpecification>();
        }
    }
}
