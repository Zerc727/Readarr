using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Core.AutoTagging;

namespace NzbDrone.Core.Datastore.Converters
{
    public class AutoTagSpecificationListConverter : JsonConverter<List<IAutoTagSpecification>>
    {
        public override void Write(Utf8JsonWriter writer, List<IAutoTagSpecification> value, JsonSerializerOptions options)
        {
            var wrapped = new List<SpecificationWrapper>();
            foreach (var spec in value)
            {
                wrapped.Add(new SpecificationWrapper { Type = spec.GetType().Name, Body = spec });
            }

            JsonSerializer.Serialize(writer, wrapped, options);
        }

        public override List<IAutoTagSpecification> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ValidateToken(reader, JsonTokenType.StartArray);

            var results = new List<IAutoTagSpecification>();

            reader.Read();

            while (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                ValidateToken(reader, JsonTokenType.PropertyName);

                reader.Read();
                ValidateToken(reader, JsonTokenType.String);
                var typename = reader.GetString();

                reader.Read();
                ValidateToken(reader, JsonTokenType.PropertyName);

                reader.Read();
                ValidateToken(reader, JsonTokenType.StartObject);

                var type = Type.GetType($"NzbDrone.Core.AutoTagging.Specifications.{typename}, Readarr.Core", true);
                var item = (IAutoTagSpecification)JsonSerializer.Deserialize(ref reader, type, options);
                results.Add(item);

                reader.Read();
                reader.Read();
            }

            ValidateToken(reader, JsonTokenType.EndArray);

            return results;
        }

        private void ValidateToken(Utf8JsonReader reader, JsonTokenType tokenType)
        {
            if (reader.TokenType != tokenType)
            {
                throw new JsonException($"Invalid token: Was expecting a '{tokenType}' token but received a '{reader.TokenType}' token");
            }
        }

        private class SpecificationWrapper
        {
            public string Type { get; set; }
            public object Body { get; set; }
        }
    }
}
