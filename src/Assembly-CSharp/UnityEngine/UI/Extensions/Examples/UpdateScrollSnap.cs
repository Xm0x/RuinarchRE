namespace UnityEngine.UI.Extensions.Examples;

public class UpdateScrollSnap : MonoBehaviour
{
	public HorizontalScrollSnap HSS;

	public VerticalScrollSnap VSS;

	public GameObject HorizontalPagePrefab;

	public GameObject VerticalPagePrefab;

	public InputField JumpPage;

	public void AddButton()
	{
		if ((bool)HSS)
		{
			GameObject gO = Object.Instantiate(HorizontalPagePrefab);
			HSS.AddChild(gO);
		}
		if ((bool)VSS)
		{
			GameObject gO2 = Object.Instantiate(VerticalPagePrefab);
			VSS.AddChild(gO2);
		}
	}

	public void RemoveButton()
	{
		if ((bool)HSS)
		{
			HSS.RemoveChild(HSS.CurrentPage, out var ChildRemoved);
			ChildRemoved.SetActive(value: false);
		}
		if ((bool)VSS)
		{
			VSS.RemoveChild(VSS.CurrentPage, out var ChildRemoved2);
			ChildRemoved2.SetActive(value: false);
		}
	}

	public void JumpToPage()
	{
		int screenIndex = int.Parse(JumpPage.text);
		if ((bool)HSS)
		{
			HSS.GoToScreen(screenIndex);
		}
		if ((bool)VSS)
		{
			VSS.GoToScreen(screenIndex);
		}
	}

	public void SelectionStartChange()
	{
		Debug.Log("Scroll Snap change started");
	}

	public void SelectionEndChange()
	{
		Debug.Log("Scroll Snap change finished");
	}

	public void PageChange(int page)
	{
		Debug.Log($"Scroll Snap page changed to {page}");
	}

	public void RemoveAll()
	{
		HSS.RemoveAllChildren(out var ChildrenRemoved);
		VSS.RemoveAllChildren(out ChildrenRemoved);
	}

	public void JumpToSelectedToggle(int page)
	{
		HSS.GoToScreen(page);
	}
}
