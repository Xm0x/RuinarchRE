namespace UnityEngine.UI.Extensions.Examples;

public class MainMenu : SimpleMenu<MainMenu>
{
	public void OnPlayPressed()
	{
		SimpleMenu<GameMenu>.Show();
	}

	public void OnOptionsPressed()
	{
		SimpleMenu<OptionsMenu>.Show();
	}

	public override void OnBackPressed()
	{
		Application.Quit();
	}
}
