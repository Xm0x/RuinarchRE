namespace UnityEngine.UI.Extensions.Examples;

public class GameMenu : SimpleMenu<GameMenu>
{
	public override void OnBackPressed()
	{
		SimpleMenu<PauseMenu>.Show();
	}
}
