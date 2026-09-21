public class Campfire : TileObject
{
	private int currentTimer;

	private int timer;

	public Campfire()
	{
		Initialize(TILE_OBJECT_TYPE.CAMPFIRE);
		AddAdvertisedAction(INTERACTION_TYPE.WARM_UP);
		timer = GameManager.Instance.GetTicksBasedOnHour(8);
	}

	public Campfire(SaveDataTileObject data)
		: base(data)
	{
		timer = GameManager.Instance.GetTicksBasedOnHour(8);
	}

	public override void SetCharacterOwner(Character newOwner)
	{
		Character character = base.characterOwner;
		base.SetCharacterOwner(newOwner);
		if (base.characterOwner != null && base.characterOwner != character)
		{
			currentTimer = 0;
		}
	}
}
