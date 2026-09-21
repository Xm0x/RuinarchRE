using Traits;

public class SaveDataHunting : SaveDataTrait
{
	public string targetAreaID;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Hunting hunting = trait as Hunting;
		targetAreaID = hunting.targetArea.persistentID;
	}
}
