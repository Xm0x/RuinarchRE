using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public class InvadeBehaviour : CharacterBehaviour
{
	public InvadeBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.behaviourComponent.invadeVillageTarget.Count <= 0)
		{
			character.behaviourComponent.ResetInvadeVillageTarget();
			PopulateVillageTargetsByPriority(character.behaviourComponent.invadeVillageTarget, character);
			if (character.behaviourComponent.invadeVillageTarget.Count <= 0)
			{
				return character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
			producedJob = null;
			return true;
		}
		Area areaLocation = character.areaLocation;
		if (areaLocation != null && character.behaviourComponent.invadeVillageTarget.Contains(areaLocation))
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			PopulateTargetChoicesFor(list, character, character.behaviourComponent.invadeVillageTarget);
			if (list.Count > 0)
			{
				Character randomElement = CollectionUtilities.GetRandomElement(list);
				character.combatComponent.Fight(randomElement, "Hostility");
			}
			else
			{
				character.behaviourComponent.ResetInvadeVillageTarget();
			}
			RuinarchListPool<Character>.Release(list);
			producedJob = null;
			return true;
		}
		LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetRandomElement(character.behaviourComponent.invadeVillageTarget).gridTileComponent.gridTiles);
		return character.jobComponent.CreateGoToSpecificTileJob(randomElement2, out producedJob);
	}

	private void PopulateTargetChoicesFor(List<Character> p_targetChoices, Character source, List<Area> p_areas)
	{
		for (int i = 0; i < p_areas.Count; i++)
		{
			p_areas[i].locationCharacterTracker.PopulateCharacterListInsideHexForInvadeBehaviour(p_targetChoices, source);
		}
	}

	private void PopulateVillageTargetsByPriority(List<Area> areas, Character owner)
	{
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		owner.currentRegion?.PopulateSettlementsInRegionForInvadeBehaviour(list);
		if (list.Count > 0)
		{
			List<BaseSettlement> list2 = RuinarchListPool<BaseSettlement>.Claim();
			for (int i = 0; i < list.Count; i++)
			{
				BaseSettlement baseSettlement = list[i];
				if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && (baseSettlement.owner == null || baseSettlement.owner.IsHostileWith(owner.faction)))
				{
					list2.Add(baseSettlement);
				}
			}
			if (list2.Count > 0)
			{
				BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list2);
				areas.AddRange(randomElement.areas);
			}
			else
			{
				List<BaseSettlement> list3 = RuinarchListPool<BaseSettlement>.Claim();
				for (int j = 0; j < list.Count; j++)
				{
					BaseSettlement baseSettlement2 = list[j];
					if (baseSettlement2.locationType == LOCATION_TYPE.DUNGEON)
					{
						list3.Add(baseSettlement2);
					}
				}
				if (list3.Count > 0)
				{
					BaseSettlement randomElement2 = CollectionUtilities.GetRandomElement(list3);
					areas.AddRange(randomElement2.areas);
				}
				RuinarchListPool<BaseSettlement>.Release(list3);
			}
			RuinarchListPool<BaseSettlement>.Release(list2);
		}
		RuinarchListPool<BaseSettlement>.Release(list);
	}
}
