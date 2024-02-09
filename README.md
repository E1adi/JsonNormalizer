# JsonNormalizer

## The WHY:
We use JSON to serialize data all the time.  
Sometimes, we want to compare two JSONs, but then we might discover that although JSON is a great format for data exchange, 
it is not ideal for data comparison.  
This is because JSON is not a canonical format, meaning that there are many different ways to represent the same data.  
 
Two simple examples for that are:
1.  Consider the following two JSONs. They represent the same data, but in different representations:
    ```json
    {
        "a": 1,
        "b": 2
    }
    ```
    ```json
    {
        "b": 2,
        "a": 1
    }
    ```
    As you might have noticed, the data is the same, but the order of the properties is different.  

2.  The other example refers more to the semantic meaning of the data that is serialiezd.  
    JSON have two types of collections; Arrays and Objects.  
    Objects are used for Objects or Dictionary like collections, while arrays are used for **lists**, **sets** etc...  
    Now, consider the following two JSONs:
    ```json
    {
        "collection": ["1", "2", "3"]
    }
    ```
    ```json
    {
        "collection": ["3", "2", "1"]
    }
    ```
    Are these two represents the same data?
    The answer is: It depends.   
    If the property `collection` represents a ***list*** of items, in which the order of items is important, then the answer is ***NO***. They are different.  
    If it represents a ***set*** of items, in which the order of items is not important, then the answer is ***YES***.
     
## The WHAT:

By normalizing the JSONs, we can make sure that the data is represented in a canonical way, so that we can compare them.  
This library provides a way to do exactly that in an efficient way.


## The HOW:

### Installation:
Add the package to you project using NuGet Package Manager or using the command line tool:
```bash
dotnet add package JsonNormalizer
```

### Usage:
The library provides a single static class `JsonNormalizer` with a single method `Normalize`.
This method accepts a `JToken` or a `string` representing a JSON and returns a normalized version of the JSON loaded into a `JToken`.   
In addition to that, you can provide a `JsonNormalizerOptions` object to control the normalization process.
Some of the options are:
-  `SortObjectsProperties`: Whether to sort the properties of objects or not.  
    Default: `true`
-  `ShouldParallelizeProcess`: Whether to parallelize the normalization process or not.  
    Default: `true`
-  `DefaultCollectionOrderSpec`: The default perspective of JSON arrays in terms of are they representing an ordered 
    collection or an unordered collection.  
    Default: `CollectionOrderSpec.Unordered`

```csharp
var json1str = """
{ 
    "key2": "value1",
    "key1": [
        {
            "innerKey2": "abc"
        },
        {	
            "innerKey1": "xyz"
        }
    ]
}
""";

var json2str = """
{ 
    "key1": [
        {	
            "innerKey1": "xyz"
        },
        {
            "innerKey2": "abc"
        }
    ],
	"key2": "value1"
}
""";

var json1 = JObject.Parse(json1str);
var json2 = JObject.Parse(json2str);
var comparer = new JTokenEqualityComparer();

comparer.Equals(json1, json2); // False

var opt = new NormalizerOptions
{
    SortObjectsProperties = true,
    ShouldParallelizeProcess = true,
    ArrayOptions = new ArrayNormalizationOptions
    {	
        // Means the order of elements in arrays (in this case; "key2") is not important
        DefaultCollectionOrderSpec = CollectionOrderSpec.Unordered
    }
};

JsonNormalizer.Normalize(json1, opt);
JsonNormalizer.Normalize(json2, opt);

Console.WriteLine(comparer.Equals(json1, json2)); // True
```