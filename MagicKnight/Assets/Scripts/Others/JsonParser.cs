using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Json;

// Parses json into a tree structure
public static class JsonParser
{
    // Use this function to parse a string into a json node tree
    public static JsonNode Parse(string jsonString)
    {
        return JsonNodeFactory.GetJsonNode(jsonString);
    }
}

// A json node. Can be object, array, or data
public abstract class JsonNode
{
    public virtual JsonNodeType type { get; }
    public JsonNode() { }
    public JsonNode(string jsonString) { }
    public abstract string GetJsonString();
    public abstract string GetDecoratedJsonString();
}

// An array node
public class JsonArrayNode : JsonNode
{
    public override JsonNodeType type { get { return JsonNodeType.ARRAY_NODE; } }
    public readonly List<JsonNode> values = new List<JsonNode>();

    public JsonArrayNode() : base() { }

    public JsonArrayNode(string jsonString) : base(jsonString)
    {
        jsonString = jsonString.CleanTrim();
        if (JsonNodeFactory.GetNodeType(jsonString) != JsonNodeType.ARRAY_NODE) return;
        // Start parsing
        List<string> jsonArray = jsonString.RemoveFirstAndLast().SafeSplit(',');
        foreach (string node in jsonArray)
        {
            // Skip empty node
            if (node.CleanTrim().Length == 0) continue;
            values.Add(JsonNodeFactory.GetJsonNode(node));
        }
    }
    
    public override string GetJsonString()
    {
        return "[" + string.Join(",", this.values.Select(it => it.GetJsonString())) + "]";
    }

    // More organized string, easier to look
    public override string GetDecoratedJsonString()
    {
        if (this.values.Count == 0) return "[]";
        return "[\n    " + string.Join(",\n", this.values.Select(it => it.GetDecoratedJsonString())).Replace("\n", "\n    ") + "\n]";
    }
}

// An object node
public class JsonObjectNode : JsonNode
{
    public override JsonNodeType type { get { return JsonNodeType.OBJECT_NODE; } }
    public readonly Dictionary<string, JsonNode> values = new Dictionary<string, JsonNode>();

    public JsonObjectNode() : base() { }

    public JsonObjectNode(string jsonString) : base(jsonString)
    {
        jsonString = jsonString.CleanTrim();
        if (JsonNodeFactory.GetNodeType(jsonString) != JsonNodeType.OBJECT_NODE) return;
        // Start parsing
        List<string> jsonArray = jsonString.RemoveFirstAndLast().SafeSplit(',');
        foreach (string node in jsonArray)
        {
            // Skip empty node
            if (node.CleanTrim().Length == 0) continue;
            List<string> pair = node.SafeSplit(':');
            Assert.AreEqual(pair.Count, 2);
            if (!typeof(string).Equals(JsonNodeFactory.GetDataType(pair[0]))) continue;
            values[pair[0].CleanTrim().RemoveFirstAndLast()] = JsonNodeFactory.GetJsonNode(pair[1]);
        }
    }

    public override string GetJsonString()
    {
        return "{" + string.Join(",", this.values.Select(it => string.Format("\"{0}\":{1}", it.Key, it.Value.GetJsonString()))) + "}";
    }

    public override string GetDecoratedJsonString()
    {
        if (this.values.Count == 0) return "{}";
        return "{\n    " + string.Join(",\n", this.values.Select(it => string.Format("\"{0}\": {1}", it.Key, it.Value.GetDecoratedJsonString()))).Replace("\n", "\n    ") + "\n}";
    }
}

// A data node. Supports float, bool, and string
// If you need int values just convert from float
// Weird values are recognized as bool(false)
public class JsonDataNode<T> : JsonNode
{
    public override JsonNodeType type { get { return JsonNodeType.DATA_NODE; } }
    public T value;

    public JsonDataNode() : base() { }

    public JsonDataNode(string jsonString) : base(jsonString)
    {
        jsonString = jsonString.CleanTrim();
        if (JsonNodeFactory.GetNodeType(jsonString) != JsonNodeType.DATA_NODE) return;
        Type t = typeof(T);
        if (typeof(string).Equals(t))
        {
            this.value = (T)(object)jsonString.RemoveFirstAndLast();
        }
        else if (typeof(float).Equals(t))
        {
            float.TryParse(jsonString, out float parsedValue);
            this.value = (T)(object)parsedValue;
        }
        else if (typeof(bool).Equals(t))
        {
            bool.TryParse(jsonString, out bool parsedValue);
            this.value = (T)(object)parsedValue;
        }
    }

    public override string GetJsonString()
    {
        return string.Format(typeof(string).Equals(typeof(T)) ? "\"{0}\"" : "{0}", this.value.ToString());
    }

    public override string GetDecoratedJsonString()
    {
        return string.Format(typeof(string).Equals(typeof(T)) ? "\"{0}\"" : "{0}", this.value.ToString());
    }
}

public enum JsonNodeType
{
    ARRAY_NODE,
    OBJECT_NODE,
    DATA_NODE
}

namespace Json
{
    // A factory for generating nodes
    internal static class JsonNodeFactory
    {
        public static JsonNodeType GetNodeType(string jsonString)
        {
            jsonString = jsonString.CleanTrim();
            if (jsonString.StartsWith("[") && jsonString.EndsWith("]")) return JsonNodeType.ARRAY_NODE;
            else if (jsonString.StartsWith("{") && jsonString.EndsWith("}")) return JsonNodeType.OBJECT_NODE;
            else return JsonNodeType.DATA_NODE;
        }

        public static Type GetDataType(string jsonString)
        {
            jsonString = jsonString.CleanTrim();
            if (JsonNodeFactory.GetNodeType(jsonString) != JsonNodeType.DATA_NODE) return null;
            else if (jsonString.StartsWith("\"") && jsonString.EndsWith("\"")) return typeof(string);
            else if (jsonString.All(it => (it >= '0' && it <= '9' || it == '.'))) return typeof(float);
            else if (!"true".Equals(jsonString.ToLower()) && !"false".Equals(jsonString.ToLower())) return typeof(bool);
            else return null;
        }

        // Generate a new json node
        public static JsonNode GetJsonNode(string jsonString)
        {
            switch (JsonNodeFactory.GetNodeType(jsonString))
            {
                case JsonNodeType.ARRAY_NODE:
                    return new JsonArrayNode(jsonString);
                case JsonNodeType.OBJECT_NODE:
                    return new JsonObjectNode(jsonString);
                case JsonNodeType.DATA_NODE:
                    Type t = JsonNodeFactory.GetDataType(jsonString);
                    if (typeof(string).Equals(t)) return new JsonDataNode<string>(jsonString);
                    else if (typeof(float).Equals(t)) return new JsonDataNode<float>(jsonString);
                    else if (typeof(bool).Equals(t)) return new JsonDataNode<bool>(jsonString);
                    break;
            }
            return null;
        }
    }

    internal static class JsonExtensionMethods
    {
        // Trim space and remove all line breaks and tabs
        internal static string CleanTrim(this string original)
        {
            return original.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
        }

        internal static string RemoveFirstAndLast(this string original)
        {
            return original.Substring(1, original.Length - 2);
        }

        /** This function separates json string
         * It avoids separating within deeper objects and data
         * For example, if the separator is ',',
         * the string `"a":["b", "c"],"d":{"e,f":"g"}` will become [`"a":["b", "c"]`, `"d":{"e,f":"g"}`]
         * The separator cannot be any of `""{}[]`
         */
        internal static List<string> SafeSplit(this string original, char separator)
        {
            List<string> result = new List<string>();
            List<int> sepLocations = new List<int>();

            int layerBraces = 0;  // record the layer of {} outside
            int layerBrackets = 0;  // record the layer of [] outside
            bool inQuotes = false;  // record whether is in quotes
            bool isEscaped = false;  // record whether is escaped

            // Go over the characters one by one to identify their role in the json
            for (int i = 0; i < original.Length; i++)
            {
                // Deal with quotes
                if (inQuotes)
                {
                    // ignore the next character if is currently escaped
                    if (isEscaped)
                    {
                        isEscaped = false;
                        continue;
                    }
                    // Inspect the value of the character
                    // Only \ and " are valid
                    if ('\\'.Equals(original[i])) isEscaped = true;
                    if ('"'.Equals(original[i])) inQuotes = false;
                }
                // Now it must be a valid character
                // Inspect the value of the character
                else if ('"'.Equals(original[i])) inQuotes = true;
                else if ('{'.Equals(original[i])) layerBraces++;
                else if ('}'.Equals(original[i])) layerBraces--;
                else if ('['.Equals(original[i])) layerBrackets++;
                else if (']'.Equals(original[i])) layerBrackets--;
                else if (separator.Equals(original[i]))
                {
                    // Only identify as sep if it is not inside braces/brackets
                    if (layerBraces == 0 && layerBrackets == 0) sepLocations.Add(i);
                }
            }

            // Now calculate the result from the list of separator locations
            int start = 0;
            foreach (int location in sepLocations)
            {
                result.Add(original.Substring(start, location - start));
                start = location + 1;
            }
            // Add the last piece of string
            result.Add(original.Substring(start));

            return result;
        }

        public static void Test()
        {
            foreach (string part in "\"asdf\": [\"aaa\", \"bbb\", 123], \"wasd\": 987, \"tc37\": {\"name\": \"daniel\", \"age\": 22}".SafeSplit(',')) UnityEngine.Debug.Log(part);
            foreach (string part in "\"aaa\", \"bbb\", 123".SafeSplit(',')) UnityEngine.Debug.Log(part);
            foreach (string part in "\"aaa\"".SafeSplit(',')) UnityEngine.Debug.Log(part);
            foreach (string part in "\"asdf\": [\"aaa\", \"bbb\", 123]".SafeSplit(':')) UnityEngine.Debug.Log(part);
            foreach (string part in "\"local-storage\": \"%HOMEDRIVE%/%HOMEPATH%/.MagicKnight\"".SafeSplit(':')) UnityEngine.Debug.Log(part);
        }
    }
}
