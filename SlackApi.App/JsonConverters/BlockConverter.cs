using SlackApi.Domain.SlackDTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable warnings

namespace SlackApi.App.JsonConverters
{
    public class BlockConverter : JsonConverter<IBlock>
    {
        public override IBlock Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                using var jsonDoc = JsonDocument.ParseValue(ref reader);
                
                var str = jsonDoc.RootElement.GetRawText();
                
                if (str == null || string.IsNullOrWhiteSpace(str))
                    return default;

                if (str.Contains("context"))
                    return JsonSerializer.Deserialize<ContextBlock>(str);

                return JsonSerializer.Deserialize<Block>(str);
            }
            catch
            {
                return default;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            IBlock value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case null:
                    JsonSerializer.Serialize(writer, (IBlock)null, options);
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
