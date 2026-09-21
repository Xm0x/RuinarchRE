using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class OrcBehaviour : BaseMonsterBehaviour
{
	public OrcBehaviour()
	{
		base.priority = 9;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		Character firstPrisonerAtHome = GetFirstPrisonerAtHome(character);
		if (firstPrisonerAtHome != null)
		{
			if (GameUtilities.RollChance(1) && character.marker.IsPOIInVision(firstPrisonerAtHome))
			{
				return character.jobComponent.TriggerTorture(firstPrisonerAtHome, out producedJob);
			}
			if (GameUtilities.RollChance(3) && character.marker.IsPOIInVision(firstPrisonerAtHome))
			{
				character.interruptComponent.TriggerInterrupt(GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, firstPrisonerAtHome);
				producedJob = null;
				return true;
			}
		}
		Character firstDeadCharacterAtHomeForButcher = GetFirstDeadCharacterAtHomeForButcher(character);
		if (firstDeadCharacterAtHomeForButcher != null && GameUtilities.RollChance(8))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, firstDeadCharacterAtHomeForButcher, character);
			goapPlanJob.SetCancelOnDeath(state: false);
			producedJob = goapPlanJob;
			return true;
		}
		if (GameUtilities.RollChance(3))
		{
			List<TileObject> list = RuinarchListPool<TileObject>.Claim();
			PopulateFoodPilesAtHome(list, character);
			FoodPile foodPile = null;
			if (list.Count > 0)
			{
				foodPile = CollectionUtilities.GetRandomElement(list) as FoodPile;
			}
			RuinarchListPool<TileObject>.Release(list);
			if (foodPile != null)
			{
				GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, foodPile, character);
				producedJob = goapPlanJob2;
				return true;
			}
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	private Character GetFirstPrisonerAtHome(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (!character2.isDead && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement) && character2.traitContainer.HasTrait("Prisoner") && character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
					{
						return character2;
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (!character3.isDead && character3.traitContainer.HasTrait("Prisoner") && character3.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner").IsConsideredPrisonerOf(character))
				{
					return character3;
				}
			}
		}
		return null;
	}

	private Character GetFirstDeadCharacterAtHomeForButcher(Character character)
	{
		if (character.homeSettlement != null && character.homeSettlement.locationType != LOCATION_TYPE.DUNGEON)
		{
			for (int i = 0; i < character.homeSettlement.areas.Count; i++)
			{
				Area area = character.homeSettlement.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++)
				{
					Character character2 = area.locationCharacterTracker.charactersAtLocation[j];
					if (character2.isDead && !character2.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(character.homeSettlement))
					{
						return character2;
					}
				}
			}
		}
		else if (character.homeStructure != null)
		{
			for (int k = 0; k < character.homeStructure.charactersHere.Count; k++)
			{
				Character character3 = character.homeStructure.charactersHere[k];
				if (character3.isDead && !character3.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER))
				{
					return character3;
				}
			}
		}
		return null;
	}
}
