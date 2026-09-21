using TMPro;
using Traits;
using UnityEngine;
using UnityEngine.UI;

public class TraitItem : MonoBehaviour
{
	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public Image iconImg;

	[SerializeField]
	private CharacterPortrait portrait;

	private Trait trait;

	public void OnHover()
	{
		if (trait != null)
		{
			string localizedName = trait.localizedName;
			localizedName = localizedName + "\n" + trait.GetTestingData();
			if (localizedName != string.Empty)
			{
				UIManager.Instance.ShowSmallInfo(localizedName);
			}
		}
	}

	public void OnHoverOut()
	{
		if (trait != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}
}
