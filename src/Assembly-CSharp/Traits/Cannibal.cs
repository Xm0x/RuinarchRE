using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class Cannibal : Trait
{
	public override bool isSingleton => true;

	public Cannibal()
	{
		name = "Cannibal";
		description = "Not a very picky eater.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		canBeTriggered = true;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character && (sourcePOI as Character).jobQueue.GetJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT) is GoapPlanJob goapPlanJob)
		{
			goapPlanJob.CancelJob();
		}
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		string result = base.TriggerFlaw(character);
		if (character.traitContainer.HasTrait("Vampire"))
		{
			Character drinkBloodTarget = GetDrinkBloodTarget(character);
			if (drinkBloodTarget != null)
			{
				if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
				{
					character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.TRIGGER_FLAW, drinkBloodTarget)?.SetIsTriggeredByPlayer(isTriggeredByPlayer);
					return result;
				}
				return "has_trigger_flaw";
			}
			return "no_target_vampire";
		}
		IPointOfInterest pOIToTransformToFood = GetPOIToTransformToFood(character);
		if (pOIToTransformToFood != null)
		{
			if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW))
			{
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.BUTCHER, pOIToTransformToFood, character);
				character.jobQueue.AddJobInQueue(job);
				return result;
			}
			return "has_trigger_flaw";
		}
		return "no_target";
	}

	private IPointOfInterest GetPOIToTransformToFood(Character characterThatWillDoJob)
	{
		IPointOfInterest pointOfInterest = null;
		for (int i = 0; i < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; i++)
		{
			Character character = characterThatWillDoJob.currentRegion.charactersAtLocation[i];
			if (characterThatWillDoJob != character && character.isDead && character.isNormalCharacter && character.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(character.gridTileLocation))
			{
				pointOfInterest = character;
				break;
			}
		}
		if (pointOfInterest == null)
		{
			for (int j = 0; j < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; j++)
			{
				Character character2 = characterThatWillDoJob.currentRegion.charactersAtLocation[j];
				if (characterThatWillDoJob != character2 && character2.isNormalCharacter && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(character2) && character2.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(character2.gridTileLocation))
				{
					pointOfInterest = character2;
					break;
				}
			}
		}
		if (pointOfInterest == null)
		{
			for (int k = 0; k < characterThatWillDoJob.currentRegion.charactersAtLocation.Count; k++)
			{
				Character character3 = characterThatWillDoJob.currentRegion.charactersAtLocation[k];
				string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(character3);
				if (characterThatWillDoJob != character3 && character3.isNormalCharacter && (opinionLabel == "Acquaintance" || string.IsNullOrEmpty(opinionLabel)) && character3.gridTileLocation != null && characterThatWillDoJob.movementComponent.HasPathTo(character3.gridTileLocation))
				{
					pointOfInterest = character3;
					break;
				}
			}
		}
		return pointOfInterest;
	}

	private Character GetDrinkBloodTarget(Character vampire)
	{
		Character result = null;
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (vampire.currentRegion != null)
		{
			for (int i = 0; i < vampire.currentRegion.charactersAtLocation.Count; i++)
			{
				Character character = vampire.currentRegion.charactersAtLocation[i];
				if (vampire != character && character.traitContainer.HasTrait("Vampire") && character.carryComponent.IsNotBeingCarried() && character.Advertises(INTERACTION_TYPE.DRINK_BLOOD) && !character.isDead && vampire.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation))
				{
					list.Add(character);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}
}
