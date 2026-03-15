namespace App.Models;

public record SimpleAppUserView
{
	public SimpleAppUserView(AppUser appUser)
	{
		ID = appUser.Id;
		Nickname = appUser.Nickname;
	}

	public string ID { get; }
	public string Nickname { get; }
}

