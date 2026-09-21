using TMPro;
using UnityEngine;

public class LocationSmallInfo : MonoBehaviour
{
	[SerializeField]
	private LocationPortrait portrait;

	[SerializeField]
	private TextMeshProUGUI nameLbl;

	[SerializeField]
	private TextMeshProUGUI typeLbl;

	[SerializeField]
	private TextMeshProUGUI subLbl;

	public Region region { get; private set; }

	public void ShowRegionInfo(Region region, string subText = "")
	{
		this.region = region;
		nameLbl.text = region.name;
		subLbl.text = subText;
		portrait.SetLocation(region);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
