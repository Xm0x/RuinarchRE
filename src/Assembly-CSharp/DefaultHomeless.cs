using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DefaultHomeless : CharacterBehaviour
{
	public DefaultHomeless()
	{
		base.priority = 26;
		attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[1] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING };
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && GameUtilities.RollChance(65, ref log) && (character.homeSettlement != null || character.faction == null || !(character.faction.leader is Character { currentJob: not null } character2) || character2.currentJob.jobType != JOB_TYPE.FIND_NEW_VILLAGE))
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
			return true;
		}
		if (character.characterClass.className != "Vampire Lord" && GameUtilities.RollChance(10) && character.homeStructure != null)
		{
			if (!(character.homeStructure is Dwelling) && !character.isVagrantOrFactionless)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
				return true;
			}
			if (character.homeStructure.structureType == STRUCTURE_TYPE.VAMPIRE_CASTLE)
			{
				bool flag = false;
				for (int i = 0; i < character.homeStructure.residents.Count; i++)
				{
					Character character3 = character.homeStructure.residents[i];
					if (character != character3 && character3.characterClass.className == "Vampire Lord" && character.relationshipContainer.HasSpecialPositiveRelationshipWith(character3))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, character);
					return true;
				}
			}
		}
		if ((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && character.currentSettlement != null && character.currentSettlement.locationType == LOCATION_TYPE.VILLAGE && character.currentSettlement != character.homeSettlement && !character.currentSettlement.HasArea(character.territory))
		{
			Area areaLocation = character.areaLocation;
			if (areaLocation != null)
			{
				LocationGridTile randomNearbyPassableWildernessGridTileWithPathTo = areaLocation.neighbourComponent.GetRandomNearbyPassableWildernessGridTileWithPathTo(character);
				if (randomNearbyPassableWildernessGridTileWithPathTo != null)
				{
					return character.jobComponent.CreateGoToJob(randomNearbyPassableWildernessGridTileWithPathTo, out producedJob);
				}
			}
		}
		return false;
	}
}
