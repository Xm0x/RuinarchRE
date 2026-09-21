namespace Locations.Settlements.Settlement_Events;

public class SaveDataWerewolfHunt : SaveDataSettlementEvent
{
	public GameDate endDate;

	public override void Save(SettlementEvent data)
	{
		base.Save(data);
		WerewolfHunt werewolfHunt = data as WerewolfHunt;
		endDate = werewolfHunt.endDate;
	}

	public override SettlementEvent Load()
	{
		return new WerewolfHunt(this);
	}
}
