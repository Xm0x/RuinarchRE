using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class RatBehaviour : CharacterBehaviour
{
	public RatBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (!character.limiterComponent.canDoFullnessRecovery)
		{
			if (character.behaviourComponent.pestSettlementTarget != null && character.behaviourComponent.pestSettlementTarget != null && character.currentRegion != null)
			{
				Area randomAreaThatIsNotMountainAndWaterAndNoSettlement = character.currentRegion.GetRandomAreaThatIsNotMountainAndWaterAndNoSettlement();
				if (randomAreaThatIsNotMountainAndWaterAndNoSettlement != null)
				{
					LocationGridTile randomPassableTile = randomAreaThatIsNotMountainAndWaterAndNoSettlement.gridTileComponent.GetRandomPassableTile();
					if (randomPassableTile != null)
					{
						character.behaviourComponent.SetPestSettlementTarget(null);
						if (character.currentStructure == randomPassableTile.structure)
						{
							return character.jobComponent.CreateGoToSpecificTileJob(randomPassableTile, out producedJob);
						}
						return character.jobComponent.CreateGoToJob(randomPassableTile, out producedJob);
					}
				}
			}
		}
		else if (character.behaviourComponent.pestHasFailedEat)
		{
			character.behaviourComponent.SetPestSettlementTarget(GetVillageTargetsByPriority(character, character.behaviourComponent.pestSettlementTarget));
			character.behaviourComponent.SetPestHasFailedEat(p_state: false);
		}
		else
		{
			if (character.behaviourComponent.pestSettlementTarget == null)
			{
				BaseSettlement currentSettlement = character.currentSettlement;
				if (currentSettlement != null && (currentSettlement.locationType == LOCATION_TYPE.VILLAGE || currentSettlement.locationType == LOCATION_TYPE.DUNGEON))
				{
					character.behaviourComponent.SetPestSettlementTarget(currentSettlement);
				}
				else
				{
					character.behaviourComponent.SetPestSettlementTarget(GetVillageTargetsByPriority(character));
				}
			}
			if (character.behaviourComponent.pestSettlementTarget != null)
			{
				BaseSettlement pestSettlementTarget = character.behaviourComponent.pestSettlementTarget;
				if (pestSettlementTarget != null)
				{
					if (character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(pestSettlementTarget))
					{
						return character.jobComponent.CreateRatFullnessRecovery(pestSettlementTarget, out producedJob);
					}
					LocationStructure randomStructure = pestSettlementTarget.GetRandomStructure();
					if (randomStructure != null)
					{
						LocationGridTile randomPassableTile2 = randomStructure.GetRandomPassableTile();
						if (randomPassableTile2 != null)
						{
							if (character.currentStructure == randomStructure)
							{
								return character.jobComponent.CreateGoToSpecificTileJob(randomPassableTile2, out producedJob);
							}
							return character.jobComponent.CreateGoToJob(randomPassableTile2, out producedJob);
						}
					}
				}
			}
		}
		return character.jobComponent.TriggerRoamAroundTile(out producedJob);
	}

	private BaseSettlement GetVillageTargetsByPriority(Character owner, BaseSettlement except = null)
	{
		BaseSettlement result = null;
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim();
		owner.currentRegion?.PopulateSettlementsInRegionThatHasAliveResidentExcept(list, owner, except);
		if (list != null)
		{
			List<BaseSettlement> list2 = RuinarchListPool<BaseSettlement>.Claim();
			for (int i = 0; i < list.Count; i++)
			{
				BaseSettlement baseSettlement = list[i];
				if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE)
				{
					list2.Add(baseSettlement);
				}
			}
			if (list2.Count > 0)
			{
				result = CollectionUtilities.GetRandomElement(list2);
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
					result = CollectionUtilities.GetRandomElement(list3);
				}
				RuinarchListPool<BaseSettlement>.Release(list3);
			}
			RuinarchListPool<BaseSettlement>.Release(list2);
		}
		RuinarchListPool<BaseSettlement>.Release(list);
		return result;
	}
}
