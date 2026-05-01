using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NzbDrone.Core.MetadataSource.OpenLibrary.Resources
{
    // OpenLibrary description/bio fields are polymorphic:
    // they can be a plain string OR {"type": "/type/text", "value": "..."}
    public class OLTextValueConverter : JsonConverter<OLTextValue>
    {
        public override OLTextValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new OLTextValue { Value = reader.GetString() };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                string value = null;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "value")
                    {
                        reader.Read();
                        value = reader.GetString();
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                return new OLTextValue { Value = value };
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, OLTextValue value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.Value);
        }
    }
}
