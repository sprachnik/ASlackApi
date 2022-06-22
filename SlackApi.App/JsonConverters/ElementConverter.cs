using SlackApi.Domain.SlackDTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable warnings

namespace SlackApi.App.JsonConverters
{
    public class ElementConverter : JsonConverter<IElement>
    {
        public override IElement Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;
            var str = root.GetRawText();

            try
            {
                if (str == null || string.IsNullOrWhiteSpace(str))
                    return default;                   

                return JsonSerializer.Deserialize<Element>(str);
            }
            catch
            {
                try
                {
                    return JsonSerializer.Deserialize<TextElement>(str);
                }
                catch
                {
                    return default;
                }
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            IElement value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (IElement)null, options);
                    break;
                default:
                    {
                        var type = value.GetType();
                        JsonSerializer.Serialize(writer, value, type, options);
                        break;
                    }
            }
        }
    }
}
