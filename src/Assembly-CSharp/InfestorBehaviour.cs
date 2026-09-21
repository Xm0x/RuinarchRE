using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class InfestorBehaviour : CharacterBehaviour
{
	public InfestorBehaviour()
	{
		base.priority = 8;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.faction != null)
		{
			JobQueueItem firstUnassignedJobToCharacterJob = character.faction.GetFirstUnassignedJobToCharacterJob(character);
			if (firstUnassignedJobToCharacterJob != null)
			{
				producedJob = firstUnassignedJobToCharacterJob;
				return true;
			}
		}
		if (!character.behaviourComponent.hasLayedAnEgg && character.gridTileLocation != null && character.gridTileLocation.tileObjectComponent.objHere == null && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory()) && Random.Range(0, 100) < 1)
		{
			int num = 0;
			num = ((character.homeSettlement == null) ? character.areaLocation.locationCharacterTracker.GetNumOfCharactersInsideHexThatHasRaceAndClassOf(character.race, character.characterClass.className) : character.homeSettlement.GetNumOfResidentsThatHasRaceAndClassOf(character.race, character.characterClass.className));
			if (num < 8)
			{
				character.jobComponent.TriggerLayEgg(out producedJob);
				return true;
			}
		}
		if (character.IsInTerritory() || character.IsInHomeSettlement() || character.isAtHomeStructure)
		{
			if (Random.Range(0, 100) < 7)
			{
				int num2 = 0;
				num2 = ((character.homeSettlement == null) ? character.areaLocation.locationCharacterTracker.GetNumOfCharactersInsideHexThatHasRaceAndClassOf(character.race, character.characterClass.className, typeof(MonsterInvadeBehaviour)) : character.homeSettlement.GetNumOfResidentsThatHasRaceAndClassOf(character.race, character.characterClass.className, typeof(MonsterInvadeBehaviour)));
				if (num2 >= 5)
				{
					Area area = null;
					List<Area> list = RuinarchListPool<Area>.Claim(10);
					character.behaviourComponent.PopulateVillageTargetsByPriority(list);
					if (list != null && list.Count > 0)
					{
						area = list[0];
					}
					RuinarchListPool<Area>.Release(list);
					if (area != null)
					{
						NPCSettlement firstNPCSettlementOnArea = area.GetFirstNPCSettlementOnArea();
						if (firstNPCSettlementOnArea != null)
						{
							return character.jobComponent.TriggerMonsterInvadeJob(firstNPCSettlementOnArea.mainStorage, out producedJob);
						}
						return character.jobComponent.TriggerMonsterInvadeJob(area, out producedJob);
					}
				}
			}
		}
		else if (character.homeStructure != null || character.HasTerritory())
		{
			character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
			return true;
		}
		character.jobComponent.TriggerRoamAroundTile(out producedJob);
		return true;
	}
}
