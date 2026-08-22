using System.Text.Json.Serialization;

namespace EkoaGym.Tools.ExerciseMedia;

// Mirrors data/exercises.schema.json from the exercises-dataset-main dataset (only the fields we need).
public class DatasetExercise
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("body_part")]
    public string BodyPart { get; set; } = string.Empty;

    [JsonPropertyName("equipment")]
    public string Equipment { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("instructions")]
    public Dictionary<string, string> Instructions { get; set; } = [];

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("gif_url")]
    public string GifUrl { get; set; } = string.Empty;
}

public class MediaMapEntry
{
    public string ImageUrl { get; set; } = string.Empty;
    public string GifUrl { get; set; } = string.Empty;
}
