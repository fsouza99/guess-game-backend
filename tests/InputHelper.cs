using System.Text.Json;

namespace App.Tests;

public static class InputHelper
{
	// JSON data folder path based on the location of execution.
	private const string WD = "..\\..\\..\\json";
	private const string SC = $"{WD}\\scorer";
	private const string VL = $"{WD}\\bad-data";

	public static JsonDocument GetJsonDocFromFile(string filename)
	{
		using (var stream = File.OpenRead(filename))
		{
			return JsonDocument.Parse(stream);
		}
	}

	// Scorer Tests
	
	public static JsonDocument GetGuessJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{SC}\\{inputReference}Gue.json");
	}

	public static JsonDocument GetReferenceJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{SC}\\{inputReference}Ref.json");
	}

	public static JsonDocument GetRulesJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{SC}\\{inputReference}Rul.json");
	}

	public static int GetScore(int inputReference)
	{
		JsonDocument doc = GetJsonDocFromFile($"{SC}\\{inputReference}Sco.json");
		return doc.RootElement.GetInt32();
	}

	// Validation Tests

	public static JsonDocument GetGoodDataTempJsonDoc(int inputReference)
	{
		return GetJsonDocFromFile($"{WD}\\GoodTempData.json");
	}

	public static JsonDocument GetBadDataJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{VL}\\{inputReference}BadData.json");
	}

	public static JsonDocument GetBadDataTempJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{VL}\\{inputReference}BadDataTemp.json");
	}

	public static JsonDocument GetBadRulesJsonDoc(int inputReference)
	{
        return GetJsonDocFromFile($"{VL}\\{inputReference}BadRules.json");
	}
}

