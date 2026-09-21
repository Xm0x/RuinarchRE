using System.Collections.Generic;

public class SaveDataExcaliburSword : SaveDataWeaponItem
{
	public List<string> traitsGainedByCurrentOwner;

	public string previousClass;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		ExcaliburSword excaliburSword = tileObject as ExcaliburSword;
		traitsGainedByCurrentOwner = new List<string>(excaliburSword.traitsGainedByCurrentOwner);
		previousClass = excaliburSword.previousClassOfCurrentOwner;
	}
}
