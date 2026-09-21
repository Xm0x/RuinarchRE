namespace UnityEngine.UI.Extensions;

public abstract class SimpleMenu<T> : Menu<T> where T : SimpleMenu<T>
{
	public static void Show()
	{
		Menu<T>.Open();
	}

	public static void Hide()
	{
		Menu<T>.Close();
	}
}
