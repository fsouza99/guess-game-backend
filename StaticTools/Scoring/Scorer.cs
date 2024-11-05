using System.Text.Json;

namespace App.StaticTools;

/// <summary>
/// Aims to obtain the score of a guess.
/// </summary>
public static class GuessScorer
{
	/// <summary>
	/// Evaluates a guess based on competition data and scoring rules.
	/// All of these need to be previously validated JSON strings.
	/// </summary>
	public static int Evaluate(string guess, string reference, string rules)
	{
		JsonElement guessRoot = JsonDocument.Parse(guess).RootElement;
		JsonElement referenceRoot = JsonDocument.Parse(reference).RootElement;
		JsonElement rulesRoot = JsonDocument.Parse(rules).RootElement;

		int total = 0;
		var properties = guessRoot.EnumerateObject();

		foreach (JsonProperty property in properties)
		{
			JsonElement guessElement = guessRoot.GetProperty(property.Name);
			JsonElement referenceElement = referenceRoot.GetProperty(property.Name);
			JsonElement rulesElement = rulesRoot.GetProperty(property.Name);

			int points = rulesElement.GetInt32();

			if (guessElement.ValueKind == JsonValueKind.Array)
			{
				var guessArray = guessElement.EnumerateArray();
				var referenceArray = referenceElement.EnumerateArray();
				foreach (var guessItem in guessArray)
				{
					string guessItemStr = guessItem.ToString();
					foreach (var referenceItem in referenceArray)
					{
						if (referenceItem.ToString() == guessItemStr)
						{
							total += points;
							break;
						}
					}
				}
			}
			else if (guessElement.ToString() == referenceElement.ToString())
			{
				total += points;
			}
		}

		return total;
	}
}

