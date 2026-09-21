using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ExterminateBehaviour : CharacterBehaviour
{
	public ExterminateBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character))
		{
			PartyQuest currentQuest = currentParty.currentQuest;
			LocationStructure locationStructure = currentQuest.target as LocationStructure;
			IPointOfInterest pointOfInterest = null;
			if (locationStructure != null)
			{
				pointOfInterest = GetRandomTargetForExtermination(locationStructure, character.faction, character);
			}
			if (pointOfInterest != null)
			{
				character.combatComponent.Fight(pointOfInterest, "Hostility");
				return true;
			}
			currentQuest?.SetIsSuccessful(state: true);
			currentParty.GoBackHomeAndEndQuest();
			return true;
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return false;
	}

	private IPointOfInterest GetRandomTargetForExtermination(LocationStructure p_targetStructure, Faction p_faction, Character p_exception)
	{
		IPointOfInterest result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		for (int i = 0; i < p_targetStructure.residents.Count; i++)
		{
			Character character = p_targetStructure.residents[i];
			if ((p_exception == null || p_exception != character) && !character.isBeingSeized && !character.isDead && character.gridTileLocation != null && character.gridTileLocation.structure == p_targetStructure && (character.faction == null || p_faction == null || p_faction.IsHostileWith(character.faction)) && !character.traitContainer.HasTrait("Hibernating", "Indestructible"))
			{
				list.Add(character);
			}
		}
		int num = list.Count - 1;
		TileObject firstTileObjectOfType = p_targetStructure.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.MONSTER_SPAWNER);
		if (firstTileObjectOfType != null)
		{
			num++;
		}
		if (num >= 0)
		{
			int num2 = GameUtilities.RandomBetweenTwoNumbers(0, num);
			if (num2 >= list.Count)
			{
				result = firstTileObjectOfType;
			}
			else if (list.Count > 0)
			{
				result = list[num2];
			}
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}
}
