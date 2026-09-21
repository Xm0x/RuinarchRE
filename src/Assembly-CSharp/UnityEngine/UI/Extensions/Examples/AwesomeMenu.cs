namespace UnityEngine.UI.Extensions.Examples;

public class AwesomeMenu : Menu<AwesomeMenu>
{
	public Image Background;

	public Text Title;

	public static void Show(float awesomeness)
	{
		Menu<AwesomeMenu>.Open();
		Menu<AwesomeMenu>.Instance.Background.color = new Color32((byte)(129f * awesomeness), (byte)(197f * awesomeness), (byte)(34f * awesomeness), byte.MaxValue);
		Menu<AwesomeMenu>.Instance.Title.text = $"This menu is {awesomeness:P} awesome";
	}

	public static void Hide()
	{
		Menu<AwesomeMenu>.Close();
	}
}
