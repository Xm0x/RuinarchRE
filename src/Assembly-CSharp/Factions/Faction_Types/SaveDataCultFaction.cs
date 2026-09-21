namespace Factions.Faction_Types;

public class SaveDataCultFaction : SaveDataFactionType
{
	public string classNameOfLeaderBeforeBecomingCultLeader;

	public override void Save(FactionType data)
	{
		base.Save(data);
		CultFaction cultFaction = data as CultFaction;
		classNameOfLeaderBeforeBecomingCultLeader = cultFaction.classNameOfLeaderBeforeBecomingCultLeader;
	}
}
