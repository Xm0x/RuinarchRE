namespace Locations.Settlements.Settlement_Events;

public class SaveDataVampireHunt : SaveDataSettlementEvent
{
	public GameDate endDate;

	public override void Save(SettlementEvent data)
	{
		base.Save(data);
		VampireHunt vampireHunt = data as VampireHunt;
		endDate = vampireHunt.endDate;
	}

	public override SettlementEvent Load()
	{
		return new VampireHunt(this);
	}
}
