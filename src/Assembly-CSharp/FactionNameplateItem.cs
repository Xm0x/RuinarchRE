using UnityEngine;
using UnityEngine.UI;

public class FactionNameplateItem : NameplateItem<Faction>
{
	[Header("General")]
	[SerializeField]
	private Image emblemImg;

	public Faction factionData { get; private set; }

	public override void SetObject(Faction o)
	{
		base.SetObject(o);
		factionData = o;
		mainLbl.text = o.nameWithColor;
		subLbl.text = o.factionType.displayName;
		emblemImg.sprite = o.emblem;
	}

	public override void Reset()
	{
		base.Reset();
		factionData = null;
	}
}
