using System;

public class StructureScroll : TileObject
{
	public static STRUCTURE_TYPE[] possibleStructures = new STRUCTURE_TYPE[4]
	{
		STRUCTURE_TYPE.LIGHTNING_TOWER,
		STRUCTURE_TYPE.ARROW_TOWER,
		STRUCTURE_TYPE.WYVERN_COOP,
		STRUCTURE_TYPE.BEAST_PEN
	};

	public STRUCTURE_TYPE structureType { get; private set; }

	public override Type serializedData => typeof(SaveDataStructureScroll);

	public StructureScroll()
	{
		Initialize(TILE_OBJECT_TYPE.STRUCTURE_SCROLL);
		AddAdvertisedAction(INTERACTION_TYPE.READ_STRUCTURE_SCROLL);
	}

	public StructureScroll(SaveDataTileObject data)
		: base(data)
	{
		SaveDataStructureScroll saveDataStructureScroll = data as SaveDataStructureScroll;
		structureType = saveDataStructureScroll.structureType;
	}

	public void SetStructureTypeToLearn(STRUCTURE_TYPE p_structure)
	{
		structureType = p_structure;
		GridMap.Instance.mainRegion.tileObjectsComponent.RemoveStructureFromStructureScrollChoices(p_structure);
	}

	private bool CanAlreadyBuildStructureThatThisScrollTeaches(Character p_character)
	{
		if (p_character.faction != null)
		{
			switch (structureType)
			{
			case STRUCTURE_TYPE.LIGHTNING_TOWER:
				return p_character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Lightning_Tower_Defense);
			case STRUCTURE_TYPE.ARROW_TOWER:
				return p_character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Tower_Defense);
			case STRUCTURE_TYPE.WYVERN_COOP:
				return p_character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Wyvern_Tamers);
			case STRUCTURE_TYPE.BEAST_PEN:
				return p_character.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Breeders);
			}
		}
		return false;
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		if (action.actor.faction != null && action.actor.faction.isMajorNonPlayer && action.goapType == INTERACTION_TYPE.READ_STRUCTURE_SCROLL)
		{
			switch (structureType)
			{
			case STRUCTURE_TYPE.LIGHTNING_TOWER:
				action.actor.faction.factionType.AddIdeology(FACTION_IDEOLOGY.Lightning_Tower_Defense, action.actor.faction);
				break;
			case STRUCTURE_TYPE.ARROW_TOWER:
				action.actor.faction.factionType.AddIdeology(FACTION_IDEOLOGY.Tower_Defense, action.actor.faction);
				break;
			case STRUCTURE_TYPE.WYVERN_COOP:
				action.actor.faction.factionType.AddIdeology(FACTION_IDEOLOGY.Wyvern_Tamers, action.actor.faction);
				break;
			case STRUCTURE_TYPE.BEAST_PEN:
				action.actor.faction.factionType.AddIdeology(FACTION_IDEOLOGY.Breeders, action.actor.faction);
				break;
			}
			Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, action.actor.faction);
		}
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (!actor.jobQueue.HasJob(JOB_TYPE.READ_SCROLL) && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.READ_SCROLL))
		{
			Faction faction = actor.faction;
			if (faction != null && faction.isMajorNonPlayer && actor.isNormalCharacter && actor.race != RACE.RATMAN && !CanAlreadyBuildStructureThatThisScrollTeaches(actor) && !HasJobTargetingThis(JOB_TYPE.READ_SCROLL))
			{
				actor.jobComponent.CreateReadScrollJob(this);
			}
		}
	}
}
