using System.Text.Json;

namespace App.StaticTools;

public static class GuessScorer
{
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

