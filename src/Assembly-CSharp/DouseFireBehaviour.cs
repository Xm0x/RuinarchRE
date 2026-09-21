using Traits;
using UnityEngine;

public class DouseFireBehaviour : CharacterBehaviour
{
	public DouseFireBehaviour()
	{
		base.priority = 950;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (StillHasFire(character))
		{
			if (!DouseNearestFire(character, out producedJob, ref log))
			{
				character.traitContainer.RemoveTrait(character, "Dousing");
			}
		}
		else
		{
			character.traitContainer.RemoveTrait(character, "Dousing");
		}
		return true;
	}

	private bool StillHasFire(Character character)
	{
		if (character.behaviourComponent.dousingFireForSettlement != null)
		{
			return character.behaviourComponent.dousingFireForSettlement.firesToDouseInSettlement.Count > 0;
		}
		return false;
	}

	private bool HasWater(Character character)
	{
		return character.HasItem(TILE_OBJECT_TYPE.WATER_FLASK);
	}

	private bool DouseNearestFire(Character character, out JobQueueItem producedJob, ref string log)
	{
		IPointOfInterest pointOfInterest = null;
		float num = 99999f;
		for (int i = 0; i < character.behaviourComponent.dousingFireForSettlement.firesToDouseInSettlement.Count; i++)
		{
			IPointOfInterest pointOfInterest2 = character.behaviourComponent.dousingFireForSettlement.firesToDouseInSettlement[i];
			Burning traitOrStatus = pointOfInterest2.traitContainer.GetTraitOrStatus<Burning>("Burning");
			if (traitOrStatus != null && traitOrStatus.douser == null && pointOfInterest2.worldObject != null && !traitOrStatus.IsResponsibleForTrait(character))
			{
				float num2 = Vector2.Distance(character.worldObject.transform.position, pointOfInterest2.worldObject.transform.position);
				if (num2 < num)
				{
					pointOfInterest = pointOfInterest2;
					num = num2;
				}
			}
		}
		if (pointOfInterest != null)
		{
			pointOfInterest.traitContainer.GetTraitOrStatus<Burning>("Burning").SetDouser(character);
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, INTERACTION_TYPE.DOUSE_FIRE, pointOfInterest, character);
			if (character.homeSettlement != null)
			{
				goapPlanJob.AddPriorityLocation(INTERACTION_TYPE.NONE, character.homeSettlement);
			}
			producedJob = goapPlanJob;
			return true;
		}
		producedJob = null;
		return false;
	}

	private void GetWater(Character character)
	{
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Water Flask", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), character, character);
		character.jobQueue.AddJobInQueue(job);
	}
}
