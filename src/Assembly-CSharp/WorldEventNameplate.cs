public class WorldEventNameplate : NameplateItem<Region>
{
	public override void SetObject(Region r)
	{
		base.SetObject(r);
		subLbl.text = string.Empty;
	}
}
