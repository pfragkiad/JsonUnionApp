using OneOf;
using OneOf.Types;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonUnion;

public static class JsonUnionSerializer
{
    public static IDictionary<string, JsonNode?>? ToDictionary(this JsonNode node)
    {
        return (node as IDictionary<string, JsonNode?>)?.ToDictionary(e=>e.Key,e=>e.Value,StringComparer.OrdinalIgnoreCase);
    }


    public static HashSet<string>? GetProperties(this JsonNode node)
    {
        var d = node.ToDictionary();
        if (d is null) return null;

        return d.Keys.Cast<string>().ToHashSet();
    }

    public static OneOf<T?, Error?>? Deserialize<T>(
        string? content,
        string propertyIdentifier,
        bool throwExceptionForBadFormat = true,
        JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        try
        {
            //json exception could appear here
            JsonNode? json = JsonNode.Parse(content);
            if (json is null) return null;

            if (string.IsNullOrWhiteSpace(propertyIdentifier))
                throw new ArgumentException(content, $"{nameof(propertyIdentifier)}");


            var d = json.ToDictionary();
            if (d is null) return null;

            if (d.ContainsKey(propertyIdentifier))
                return json.Deserialize<T>(options);


            return new Error(Message: $"Could not parse JSON text. Value: '{content}'");
        }
        catch (JsonException e)
        {
            if (throwExceptionForBadFormat) throw e;
            return new Error(Message: e.Message);
        }
    }

    public static OneOf<TFirst?, TSecond?, Error?>? Deserialize<TFirst, TSecond>(
        string? content,
        string propertyIdentifierForFirstType,
        string propertyIdentifierForSecondType,
        bool throwExceptionForBadFormat = true,
        JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        if (string.IsNullOrWhiteSpace(propertyIdentifierForFirstType))
            throw new ArgumentException(content, $"{nameof(propertyIdentifierForFirstType)}");


        if (string.IsNullOrWhiteSpace(propertyIdentifierForSecondType))
            throw new ArgumentException(content, $"{nameof(propertyIdentifierForSecondType)}");


        //if we arrive here at least one of the propertyIdentifierForFirstType, propertyIdentifierForSecondType are not null

        try
        {
            //json exception could appear here
            JsonNode? json = JsonNode.Parse(content);
            if (json is null) return null;

            var d = json.ToDictionary()?.ToDictionary(e=>e.Key,e=>e.Value,StringComparer.InvariantCultureIgnoreCase);
            if (d is null) return null;

            if (d.ContainsKey(propertyIdentifierForFirstType))
                return json.Deserialize<TFirst>(options);

            if (d.ContainsKey(propertyIdentifierForSecondType))
                return json.Deserialize<TSecond>(options);

            else return new Error(Message: $"Could not parse JSON text. Value: '{content}'");

        }
        catch (JsonException e)
        {
            if (throwExceptionForBadFormat) throw e;
            return new Error(Message: e.Message);
        }


    }
}