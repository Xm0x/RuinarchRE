using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UtilityScripts;

public class RaidBehaviour : CharacterBehaviour
{
	public RaidBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool flag = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working && currentParty.targetDestination.IsAtTargetDestination(character))
		{
			RaidPartyQuest raidPartyQuest = currentParty.currentQuest as RaidPartyQuest;
			if (raidPartyQuest.target == null)
			{
				currentParty.GoBackHomeAndEndQuest();
				return true;
			}
			if (currentParty.targetDestination is BaseSettlement baseSettlement)
			{
				if (GameUtilities.RollChance(40, ref log) && character.areaLocation.HasSettlementOnArea(baseSettlement))
				{
					TileObject randomArsonTarget = GetRandomArsonTarget(character, character.areaLocation);
					if (randomArsonTarget != null)
					{
						flag = character.jobComponent.TriggerArsonRaid(randomArsonTarget, ref producedJob);
						if (flag)
						{
							raidPartyQuest.SetIsSuccessful(state: true);
						}
					}
				}
				if (producedJob == null)
				{
					LocationStructure randomDwellingOrResourceProducingStructure = baseSettlement.GetRandomDwellingOrResourceProducingStructure();
					if (randomDwellingOrResourceProducingStructure != null)
					{
						LocationGridTile randomPassableTile = randomDwellingOrResourceProducingStructure.GetRandomPassableTile();
						flag = ((randomPassableTile == null) ? character.jobComponent.TriggerRoamAroundStructure(out producedJob) : character.jobComponent.CreateGoToSpecificTileJob(randomPassableTile, out producedJob));
					}
					else
					{
						flag = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
					}
				}
			}
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return flag;
	}

	private Character GetRandomAliveResidentInsideSettlementThatIsHostileWith(Character character, BaseSettlement settlement)
	{
		List<Character> list = null;
		for (int i = 0; i < settlement.residents.Count; i++)
		{
			Character character2 = settlement.residents[i];
			if (character != character2 && !character2.isDead && !character2.isBeingSeized && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(settlement) && (character2.faction == null || character.faction == null || character.faction.IsHostileWith(character2.faction)))
			{
				if (list == null)
				{
					list = new List<Character>();
				}
				list.Add(character2);
			}
		}
		if (list != null && list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}

	private TileObject GetRandomArsonTarget(Character actor, Area a)
	{
		TileObject result = null;
		if (a != null)
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			for (int i = 0; i < a.tileObjectComponent.itemsInArea.Count; i++)
			{
				TileObject tileObject = a.tileObjectComponent.itemsInArea[i];
				if (tileObject != null && !tileObject.traitContainer.HasTrait("Burning", "Fire Resistant") && tileObject.traitContainer.HasTrait("Flammable") && !tileObject.isBeingSeized)
				{
					list.Add(tileObject);
				}
			}
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<TileObject>.Release(list);
		}
		return result;
	}
}
