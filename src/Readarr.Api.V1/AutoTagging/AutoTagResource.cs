using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AutoTagging;
using Readarr.Http.REST;

namespace Readarr.Api.V1.AutoTagging
{
    public class AutoTagResource : RestResource
    {
        public string Name { get; set; }
        public bool RemoveTags { get; set; }
        public List<int> Tags { get; set; }
        public List<AutoTagSpecificationSchema> Specifications { get; set; }
    }

    public class AutoTagSpecificationSchema
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }
        public object Fields { get; set; }
    }

    public static class AutoTagResourceMapper
    {
        public static AutoTagResource ToResource(this AutoTag model)
        {
            if (model == null)
            {
                return null;
            }

            return new AutoTagResource
            {
                Id = model.Id,
                Name = model.Name,
                RemoveTags = model.RemoveTags,
                Tags = model.Tags,
                Specifications = model.Specifications.Select(s => new AutoTagSpecificationSchema
                {
                    Name = s.Name,
                    Implementation = s.ImplementationName,
                    Negate = s.Negate,
                    Required = s.Required
                }).ToList()
            };
        }

        public static AutoTag ToModel(this AutoTagResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new AutoTag
            {
                Id = resource.Id,
                Name = resource.Name,
                RemoveTags = resource.RemoveTags,
                Tags = resource.Tags ?? new List<int>(),
                Specifications = new List<IAutoTagSpecification>()
            };
        }

        public static List<AutoTagResource> ToResource(this IEnumerable<AutoTag> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
