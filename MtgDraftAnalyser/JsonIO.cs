using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Model;

namespace IO;

internal static class JsonSerializerSettings
{
    public static JsonSerializerOptions SerializerOptions
    {
        get
        {
            var jsonSerialisationOptions = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = true,
                WriteIndented = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            jsonSerialisationOptions.Converters.Add(new CardJsonConverter());
            return jsonSerialisationOptions;
        }
    }
}

internal class CardJsonConverter : JsonConverter<Card>
{
    public override Card Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token but got {reader.TokenType}");
        }
        var cardName = reader.GetString()!;
        return new Card(cardName);
    }

    public override void Write(Utf8JsonWriter writer, Card value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.CardName);
    }
}

public static class JsonIO
{
    private static string JsonFilePath(string fileName) => $"./17lands/{fileName}.json";

    public static IList<Draft> DeserializeDraftsFromFile(string fileName)
    {
        using var fileStream = File.OpenRead(JsonFilePath(fileName));
        var draftData = JsonSerializer.Deserialize<IList<Draft>>(
            fileStream,
            JsonSerializerSettings.SerializerOptions) ?? [];
        return draftData;
    }

    public static void SerializeDraftsToFile(string fileName, IList<Draft> drafts)
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(
            stream,
            drafts,
            drafts.GetType(),
            JsonSerializerSettings.SerializerOptions);

        using var fileStream = File.Create(JsonFilePath(fileName));
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(fileStream);
    }
}