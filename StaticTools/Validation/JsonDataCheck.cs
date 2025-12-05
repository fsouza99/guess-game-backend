using System.Text.Json;

namespace App.StaticTools;

/// <summary>
/// Provides static methods for JSON data evaluation, developed for the JSON data
/// expected to be persisted on "Guess", "Formula", "Game" and "Competition" tables.
/// </summary>
public static class JsonDataChecker
{
    /// <summary>
    /// Check whether an element is in accordance with data template.
    /// The template element is expected to express the size of the array of strings
    /// the element should be.
    /// </summary>
    private static bool ElementOnTemplate(
        JsonElement element, JsonElement template)
    {
        int expectedSize = template.GetInt32();

        // If template expects a string, check it.
        if (expectedSize == 1)
        {
            return element.ValueKind == JsonValueKind.String;
        }

        // Otherwise, check if element is an array.
        if (element.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        // Check array size.
        int elementArraySize = element.GetArrayLength();
        if (expectedSize != elementArraySize)
        {
            return false;
        }

        // Make sure inner elements are strings.
        for (int i = 0; i < elementArraySize; i++)
        {
            if (element[i].ValueKind != JsonValueKind.String)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check whether an element is an integer within an interval.
    /// </summary>
    private static bool IsLimitedInt32(JsonElement element, int min, int max)
    {
        if (element.ValueKind == JsonValueKind.Number
            && element.TryGetInt32(out int value))
        {
            return value >= min && value <= max;
        }
        return false;
    }

    /// <summary>
    /// Check the conformance of data with a template.
    /// The JSON of the template is expected to be valid.
    /// </summary>
    public static bool DataOnTemplate(JsonDocument data, JsonDocument template)
    {
        JsonElement templateRoot = template.RootElement;
        JsonElement dataRoot = data.RootElement;

        // Data must be an object.
        if (dataRoot.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var templateProperties = templateRoot.EnumerateObject();
        var dataProperties = dataRoot.EnumerateObject();

        // Check the number of properties.
        if (templateProperties.Count() != dataProperties.Count())
        {
            return false;
        }

        // Every data property must exist in template.
        var propNames = templateProperties.Select(p => p.Name);
        foreach (string propName in propNames)
        {
            if (!dataRoot.TryGetProperty(propName, out JsonElement dataElement))
            {
                return false;
            }

            JsonElement templateElement = templateRoot.GetProperty(propName);
            if (!ElementOnTemplate(dataElement, templateElement))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validates JSON data representing a template for competition data.
    /// The JSON data must include a non-empty root node.
    /// Values can only be integers in the [1,100] interval.
    /// </summary>
    public static bool DataTemplate(JsonDocument template)
    {
        JsonElement templateRoot = template.RootElement;

        // Template must be an object.
        if (templateRoot.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        // Template must contain at least 1 element.
        var templateProperties = templateRoot.EnumerateObject();
        if (!templateProperties.Any())
        {
            return false;
        }

        // Values can only be limited integers.
        var valueElements = templateProperties.Select(p => p.Value);
        foreach (JsonElement element in valueElements)
        {
            if (!IsLimitedInt32(element, 1, 100))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks the conformance of data representing scoring rules with the
    /// data template expected by a formula.
    /// The JSON of the template is expected to be valid.
    /// Values can only be integers in the [0,1000] interval.
    /// </summary>
    public static bool ScoringRulesOnTemplate(
        JsonDocument rules, JsonDocument template)
    {
        JsonElement rulesRoot = rules.RootElement;
        JsonElement templateRoot = template.RootElement;

        // Root must be an object.
        if (rulesRoot.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var templateProperties = templateRoot.EnumerateObject();
        var rulesProperties = rulesRoot.EnumerateObject();

        // Check the number of properties.
        if (templateProperties.Count() != rulesProperties.Count())
        {
            return false;
        }

        // Every data property must exist in template.
        var propNames = templateProperties.Select(p => p.Name);
        foreach (string propName in propNames)
        {
            if (!rulesRoot.TryGetProperty(propName, out JsonElement ruleElement))
            {
                return false;
            }
            if (!IsLimitedInt32(ruleElement, 0, 1000))
            {
                return false;
            }
        }

        return true;
    }
}
