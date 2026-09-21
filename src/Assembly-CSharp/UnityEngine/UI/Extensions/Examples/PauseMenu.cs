namespace UnityEngine.UI.Extensions.Examples;

public class PauseMenu : SimpleMenu<PauseMenu>
{
	public void OnQuitPressed()
	{
		SimpleMenu<PauseMenu>.Hide();
		Object.Destroy(base.gameObject);
		SimpleMenu<GameMenu>.Hide();
	}
}
