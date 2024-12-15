using System.Text.Json;

namespace App.StaticTools;

/// <summary>
/// Provides static methods for JSON data evaluation, targeted on the JSON data
/// expected to be persisted on "Guess", "Formula", "Game" and "Competition" tables.
/// </summary>
public static class JsonDataChecker
{
	private static bool ElementOnDataTemplate(JsonElement template, JsonElement element)
	{
		// Element's type must match the template's.
		if (template.ValueKind != element.ValueKind)
		{
			return false;
		}

		// If element is an array...
		if (template.ValueKind == JsonValueKind.Array)
		{
			// Check array size.
			int size = template.GetArrayLength();
			if (size != element.GetArrayLength())
			{
				return false;
			}
			
			// Check inner elements.
			for (int i = 0; i < size; i++)
			{
				if (template[i].ValueKind != element[i].ValueKind)
				{
					return false;
				}
			}
		}

		return true;
	}

	private static bool IsNullObjUndef(JsonElement element)
	{
		return (
			element.ValueKind == JsonValueKind.Null ||
			element.ValueKind == JsonValueKind.Object ||
			element.ValueKind == JsonValueKind.Undefined
			);
	}

	private static bool IsLimitedInt32(JsonElement element, int min, int max)
	{
		int valueInt;
		try
		{
			valueInt = element.GetInt32();
		}
		catch (Exception)
		{
			return false;
		}
		return (valueInt >= min && valueInt <= max);
	}

	/// <summary>
	/// Checks the conformance of JSON data with a template.
	/// The JSON of the template is expected to be valid.
	/// </summary>
	public static bool DataOnTemplate(JsonDocument template, JsonDocument data)
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
		foreach (JsonProperty property in templateProperties)
		{
			JsonElement templateElement = templateRoot.GetProperty(property.Name), dataElement;
			try
			{
				dataElement = dataRoot.GetProperty(property.Name);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}

			// Check element.
			if (!ElementOnDataTemplate(templateElement, dataElement))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Validates JSON data representing a template for competition data.
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
		if (templateProperties.Count() == 0)
		{
			return false;
		}

		foreach (JsonProperty property in templateProperties)
		{
			JsonElement element = property.Value;

			// A property cannot be of type "null", "object" nor "undefined".
			if (IsNullObjUndef(element))
			{
				return false;
			}

			// An array can only contain simple types as well, and no inner arrays.
			if (element.ValueKind == JsonValueKind.Array)
			{
				var elements = element.EnumerateArray();
				foreach (var item in elements)
				{
					if (
						item.ValueKind == JsonValueKind.Array ||
						IsNullObjUndef(item)
						)
					{
						return false;
					}
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Validates JSON data representing a template for scoring rules.
	/// </summary>
	public static bool ScoringRulesTemplate(JsonDocument template)
	{
		JsonElement templateRoot = template.RootElement;

		// Template must be an object.
		if (templateRoot.ValueKind != JsonValueKind.Object)
		{
			return false;
		}

		// Template must contain at least 1 element.
		var templateProperties = templateRoot.EnumerateObject();
		if (templateProperties.Count() == 0)
		{
			return false;
		}

		// An element must be an integer in [0, 1000].
		foreach (JsonProperty property in templateProperties)
		{
			JsonElement element = property.Value;
			if (!IsLimitedInt32(element, 0, 1000))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks the conformance of JSON data representing scoring rules with a template.
	/// The JSON of the template is expected to be valid.
	/// </summary>
	public static bool ScoringRulesOnTemplate(JsonDocument template, JsonDocument data, int min=0, int max=1000)
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
		foreach (JsonProperty property in templateProperties)
		{
			JsonElement templateElement = templateRoot.GetProperty(property.Name), dataElement;
			try
			{
				dataElement = dataRoot.GetProperty(property.Name);
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
			if (!IsLimitedInt32(dataElement, 0, 1000))
			{
				return false;
			}
		}

		return true;
	}
}
