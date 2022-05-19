using System.Text.Json.Serialization;

namespace SlackApi.Domain.DTOs
{
    public class Block
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("block_id")]
        public string? BlockId { get; set; }

        [JsonPropertyName("accessory")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Accessory? Accessory { get; set; }

        [JsonPropertyName("label")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Label? Label { get; set; }

        [JsonPropertyName("text")]
        public Text? Text { get; set; }

        [JsonPropertyName("elements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Element>? Elements { get; set; }

        [JsonPropertyName("fields")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Field>? Fields { get; set; }

        [JsonPropertyName("element")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Element? Element { get; set; }

        [JsonPropertyName("optional")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Optional { get; set; }
    }

    public class Text
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? BlockText { get; set; }

        [JsonPropertyName("verbatim")]
        public bool Verbatim { get; set; }

        [JsonPropertyName("emoji")]
        public bool Emoji { get; set; }
    }

    public class Element
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("action_id")]
        public string? ActionId { get; set; }

        [JsonPropertyName("text")]
        public Text? Text { get; set; }

        [JsonPropertyName("placeholder")]
        public Placeholder? Placeholder { get; set; }

        [JsonPropertyName("multiline")]
        public bool? Multiline { get; set; }
    }

    public class Placeholder
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class Field
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class Accessory
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public ViewText? Text { get; set; }

        [JsonPropertyName("action_id")]
        public string? ActionId { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("style")]
        public string? Style { get; set; }
    }

    public class Label
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class ViewText
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
