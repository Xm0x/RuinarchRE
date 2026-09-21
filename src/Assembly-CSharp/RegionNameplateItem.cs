using UnityEngine;

public class RegionNameplateItem : NameplateItem<Region>
{
	[Header("Region Attributes")]
	[SerializeField]
	private LocationPortrait portrait;

	private Region region;

	public override Region obj => region;

	public override void SetObject(Region o)
	{
		base.SetObject(o);
		region = o;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		portrait.SetLocation(region);
		mainLbl.text = region.name;
		subLbl.text = string.Empty;
	}

	public override void Reset()
	{
		base.Reset();
		region = null;
	}
}
