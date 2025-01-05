namespace App.StaticTools;

public static class DataGen
{
	public static string GenerateID()
	{
		const string alphanum = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		var buffer = new char[11];
		var dice = new Random();
		for (int i = 0; i < 11; i++)
		{
			buffer[i] = alphanum[dice.Next(62)];
		}
		return new String(buffer);
	}
}
