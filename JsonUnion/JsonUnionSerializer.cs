using OneOf;
using OneOf.Types;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonUnion;

public static class JsonUnionSerializer
{
    public static IDictionary<string, JsonNode?>? ToDictionary(this JsonNode node)
    {
        return node as IDictionary<string, JsonNode?>;
    }


    public static HashSet<string>? GetProperties(this JsonNode node)
    {
        var d = node.ToDictionary();
        if (d is null) return null;

        return d.Keys.Cast<string>().ToHashSet();
    }

    public static OneOf<T?, Error?>? Deserialize<T>(string content, bool throwExceptionForBadFormat = true, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(content, nameof(content));

        try
        {
            //json exception could appear here
            JsonNode? json = JsonNode.Parse(content);
            if (json is null) return null;

            var d = json.ToDictionary();
            if (d is null) return null;

            return json.Deserialize<T>(options);
        }
        catch (JsonException e)
        {
            if (throwExceptionForBadFormat) throw e;
            return new Error(Message: e.Message);
        }
    }

    public static OneOf<TFirst?, TSecond?, Error?>? Deserialize<TFirst, TSecond>(string content, string? propertyIndentifierForFirstType, string? propertyIdentifierForSecondType = null, bool throwExceptionForBadFormat = true, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(content, nameof(content));
        if (string.IsNullOrWhiteSpace(propertyIndentifierForFirstType) && string.IsNullOrWhiteSpace(propertyIdentifierForSecondType))
            ArgumentException.ThrowIfNullOrEmpty(content, $"{nameof(propertyIndentifierForFirstType)}|{nameof(propertyIdentifierForSecondType)}");

        //if we arrive here at least one of the propertyIndentifierForFirstType, propertyIdentifierForSecondType are not null

        try
        {
            //json exception could appear here
            JsonNode? json = JsonNode.Parse(content);
            if (json is null) return null;

            var d = json.ToDictionary();
            if (d is null) return null;

            if (propertyIndentifierForFirstType is not null && d.ContainsKey(propertyIndentifierForFirstType))
                return json.Deserialize<TFirst>(options);

            //propertyIdentifierForSecondType is not null
            return json.Deserialize<TSecond>(options);

        }
        catch (JsonException e)
        {
            if (throwExceptionForBadFormat) throw e;
            return new Error(Message: e.Message);
        }


    }
}