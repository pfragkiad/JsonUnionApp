using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using JsonUnion;

namespace JsonTests;

internal class Program
{

    public class ResponseOk
    {
        public int Prop1 { get; init; }
        public string? Prop2 { get; init; }
        public string? Prop3 { get; init; }
    }
    public class ResponseOk2
    {
        public string? Message { get; init; }
        public int Code { get; init; }
    }

    static void Main(string[] args)
    {

        string contentScenario1 = """
                {
                "prop1": 10,
                "prop2": "n1"
                }
                """;

        string contentScenario2 = """
                {
                "message": "Alternative stuff",
                "code": 100
                }
                """;

        JsonSerializerOptions o = new() { PropertyNameCaseInsensitive = true };

        OneOf.OneOf<ResponseOk?, ResponseOk2?, Error?>? response1 = JsonUnionSerializer.Deserialize<ResponseOk, ResponseOk2>(
            contentScenario1,
            propertyIndentifierForFirstType: "prop1",
            throwExceptionForBadFormat: false,
            options: o);

        if (response1.HasValue)
        {
            var r = response1.Value;
            if (r.IsT0) //i.e. ResponseOk
            {
                ResponseOk ok = r.AsT0!;
                Console.WriteLine($"{ok.Prop1}, {ok.Prop2}");
            }
            else if (r.IsT1) //i.e. ResponseOk2
            {
                ResponseOk2 ok2 = r.AsT1!;
                Console.WriteLine($"{ok2.Message}, {ok2.Code}");
            }
            else //Error
            {
                Error error = r.AsT2!;
                Console.WriteLine($"{error.Message}");
            }
        }
        //will print:
        //10, n1


        if (response1.HasValue)
        {
            var r = response1.Value;
            r.Switch(
                ok => Console.WriteLine($"{ok!.Prop1}, {ok!.Prop2}"),
                ok2 => Console.WriteLine($"{ok2!.Message}, {ok2.Code}"),
                error => Console.WriteLine($"{error!.Message}")
                );
        }
        //will print:
        //Alternative stuff, 100


        //Example 2: Get properties/keys of returned json

        //Get only the properties of the JSON node, not the non-initialized properties of the instance.
        JsonNode? jsonNode1 = JsonNode.Parse(contentScenario1);
        var properties = jsonNode1!.GetProperties()!;
        foreach (string p in properties)
            Console.WriteLine(p);
        //will print:
        //prop1
        //prop2

        var d = jsonNode1!.ToDictionary();
        Console.WriteLine(d!.ContainsKey("prop1"));


    }
}