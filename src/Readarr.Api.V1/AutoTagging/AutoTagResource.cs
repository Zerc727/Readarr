using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AutoTagging;
using Readarr.Http.ClientSchema;
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

    public class AutoTagSpecificationSchema : RestResource
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string ImplementationName { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }
        public List<Field> Fields { get; set; }
        public List<AutoTagSpecificationSchema> Presets { get; set; }
    }

    public static class AutoTagSpecificationSchemaMapper
    {
        public static AutoTagSpecificationSchema ToSchema(this IAutoTagSpecification model)
        {
            return new AutoTagSpecificationSchema
            {
                Name = model.Name,
                Implementation = model.GetType().Name,
                ImplementationName = model.ImplementationName,
                Negate = model.Negate,
                Required = model.Required,
                Fields = SchemaBuilder.ToSchema(model)
            };
        }
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
                Specifications = model.Specifications.Select(s => s.ToSchema()).ToList()
            };
        }

        public static AutoTag ToModel(this AutoTagResource resource, List<IAutoTagSpecification> specifications)
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
                Specifications = resource.Specifications?
                    .Select(s => MapSpecification(s, specifications))
                    .ToList() ?? new List<IAutoTagSpecification>()
            };
        }

        public static List<AutoTagResource> ToResource(this IEnumerable<AutoTag> models)
        {
            return models.Select(ToResource).ToList();
        }

        private static IAutoTagSpecification MapSpecification(AutoTagSpecificationSchema schema, List<IAutoTagSpecification> specifications)
        {
            var match = specifications.SingleOrDefault(x => x.GetType().Name == schema.Implementation);

            if (match is null)
            {
                throw new ArgumentException($"{schema.Implementation} is not a valid AutoTag specification");
            }

            var spec = (IAutoTagSpecification)SchemaBuilder.ReadFromSchema(schema.Fields, match.GetType());
            spec.Name = schema.Name;
            spec.Negate = schema.Negate;
            spec.Required = schema.Required;
            return spec;
        }
    }
}
