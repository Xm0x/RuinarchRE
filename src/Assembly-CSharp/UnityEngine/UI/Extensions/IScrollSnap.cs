namespace UnityEngine.UI.Extensions;

internal interface IScrollSnap
{
	void ChangePage(int page);

	void SetLerp(bool value);

	int CurrentPage();

	void StartScreenChange();
}
