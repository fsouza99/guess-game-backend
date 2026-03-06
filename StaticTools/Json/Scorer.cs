using System.Text.Json;

namespace App.StaticTools;

/// <summary>
/// Provides a main method to evaluate a guess according to reference data and
/// a set of scoring rules.
/// </summary>
public static class GuessScorer
{
	/// <summary>
	/// Evaluate array in guess data according to reference.
	/// The returned value equals the number of common items on guess and
	/// reference arrays multiplied by the informed points.
	/// </summary>
	private static int EvaluateArrayElement(
		JsonElement guessElement, JsonElement referenceElement, int points)
	{
		var guessArray = guessElement.EnumerateArray();
		var referenceArray = referenceElement.EnumerateArray();
		int total = 0;
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
		return total;
	}

	/// <summary>
	/// Evaluate a guess according to reference data and a set of scoring rules.
	/// </summary>
	public static int Evaluate(JsonDocument guess, JsonDocument reference, JsonDocument rules)
	{
		JsonElement guessRoot = guess.RootElement;
		JsonElement referenceRoot = reference.RootElement;
		JsonElement rulesRoot = rules.RootElement;

		int total = 0;
		var propNames = guessRoot.EnumerateObject().Select(p => p.Name);
		foreach (string propName in propNames)
		{
			JsonElement guessElement = guessRoot.GetProperty(propName);
			JsonElement referenceElement = referenceRoot.GetProperty(propName);
			JsonElement rulesElement = rulesRoot.GetProperty(propName);

			int points = rulesElement.GetInt32();

			if (guessElement.ValueKind == JsonValueKind.Array)
			{
				total += EvaluateArrayElement(guessElement, referenceElement, points);
			}
			else if (guessElement.ToString() == referenceElement.ToString())
			{
				total += points;
			}
		}

		return total;
	}
}

