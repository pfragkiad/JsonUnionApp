# JsonUnion

*The simplest way for multiple JSON responses under a single request.*

Combine the power the `OneOf` library with union-like returns of JSON responses. 

## How to install

Via tha Package Manager:
```powershell
Install-Package JsonUnion
```

Via the .NET CLI
```bat
dotnet add package JsonUnion
```
## Example 1 - Get the properties of a JSON node

The extension method is useful when a single endpoint returns JSON answers with different formats.
In the following example, we assume a class named `ResponseOk`. We want to examine the properties of the JSON string before assigning it to a typed variable.

```cs
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonUnion;

public class ResponseOk
{
    public int Prop1 { get; init; }
    public string? Prop2 { get; init; }
    public string? Prop3 { get; init; }
}
...

string contentScenario1 = """
    {
    "prop1": 10,
    "prop2": "n1"
    }
    """;


    //Case 1
    //Get only the properties of the JSON node, not the non-initialized properties of the instance.
    JsonNode? jsonNode1 = JsonNode.Parse(contentScenario1);
    //GetProperties is an extension method of JsonUnion library
    var properties = jsonNode1!.GetProperties()!; 
    foreach (string p in properties)
        Console.WriteLine(p);

    //will print:
    //prop1
    //prop2
```

## Example 2 - Examine if a property of JSON node exists

For the same variable `contentScenario1`, we can get an `IDictionary`, which allows examination of the existence of a specific property.
By ensuring that a property exists, we can assume that the correct JSON format is returned, because the parse is completed. The returned dictionary is case insensitive.

```cs
//Case 2
JsonNode? jsonNode1 = JsonNode.Parse(contentScenario1);

//ToDictionary is an extension method of JsonUnion library
var d = jsonNode1!.ToDictionary();
Console.WriteLine(d!.ContainsKey("prop1"));

//will print:
//true

```
## Example 3 - Return one of multiple types avoiding throwing exceptions

The `Deserialize` method of the `JsonSerializer` throws an exception when the expected format is problematic.
This is where the `JsonUnion` comes into play.
In the following example, we assume that there are 2 possible return types. The `Deserialize` method of the `JsonUnionSerializer` static class returns a `OneOf` instance.

More specifically this instance is of type: `OneOf.OneOf<ResponseOk?, ResponseOk2?, Error?>?`
The `Error` type is a simpe record type that encapsulates a single error message in case for parse exceptions only.
If the `content` argument is null/empty/whitespace then `null` is returned.
The `propertyIdentifierForFirstType` argument is the property identifier of the first candidate type (in this case of type `ResponseOk`). It is assumed that this property is not also contained in the `ResponseOk2`.
The `propertyIdentifierForSecondType` correspondingly is the property identifier of the second type. Both the identifiers are treated as case insensitive.
The `propertyIdentifierForFirstType` and `propertyIdentifierForSecondType` must be non empty or else the call is meaningless. An `ArgumentException` is thrown if either of the two arguments are empty.


```cs
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
...

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

var response1 = JsonUnionSerializer.Deserialize<ResponseOk, ResponseOk2>(
    content: contentScenario1,
    propertyIdentifierForFirstType: "prop1",
    propertyIdentifierForSecondType: "message",
    throwExceptionForBadFormat: false,
    options: o);



//example of handling the response
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

//will print (for contentScenario1):
//10, n1
```

We can do things more compactly using either the `Switch` or the `Match` methods, as shown below:
```cs
//Switch example
if (response1.HasValue)
{
    var r = response1.Value;
    r.Switch(
        ok => Console.WriteLine($"{ok!.Prop1}, {ok!.Prop2}"),
        ok2 => Console.WriteLine($"{ok2!.Message}, {ok2.Code}"),
        error => Console.WriteLine($"{error!.Message}")
        );
}

//Match example
if (response1.HasValue)
{
    var r = response1.Value;
    int intResult = r.Match(
        ok => ok!.Prop1,
        ok2 => ok2!.Code,
        error => -1
        );
}
```

If you want to see more things about the `OneOf` library, you are strongly encourages to check [here](https://www.nuget.org/packages/OneOf).
