namespace App.StaticTools;

public static class DataGen
{
	public static string GenerateID()
	{
		const string alphanum = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		const int size = 11;
		var buffer = new char[size];
		var dice = new Random();
		for (int i = 0; i < size; i++)
		{
			buffer[i] = alphanum[dice.Next(62)];
		}
		return new String(buffer);
	}
}
