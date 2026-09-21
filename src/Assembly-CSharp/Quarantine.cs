using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class Quarantine : GoapAction
{
	public Quarantine()
		: base(INTERACTION_TYPE.QUARANTINE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
		base.actionIconString = GoapActionStateDB.Cure_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Social
		};
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.CARRIED_PATIENT, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsPatientCarried);
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Quarantine Success", goapNode);
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi);
	}

	public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode)
	{
		Character targetCharacter = goapNode.poiTarget as Character;
		return GetValidBedForActor(goapNode.actor, targetCharacter);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = node.poiTarget as Character;
			BedClinic bedClinic = node.actor.gridTileLocation.tileObjectComponent.objHere as BedClinic;
			if (bedClinic == null)
			{
				for (int i = 0; i < node.actor.gridTileLocation.neighbourList.Count; i++)
				{
					if (node.actor.gridTileLocation.neighbourList[i].tileObjectComponent.objHere is BedClinic bedClinic2 && bedClinic2.IsAvailable() && bedClinic2.CanUseBed(character))
					{
						bedClinic = bedClinic2;
						break;
					}
				}
			}
			if (bedClinic == null)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_space_bed";
			}
		}
		return goapActionInvalidity;
	}

	public override void OnInvalidAction(ActualGoapNode node)
	{
		base.OnInvalidAction(node);
		Character actor = node.actor;
		Character poi = node.poiTarget as Character;
		actor.UncarryPOI(poi);
	}

	private bool IsPatientCarried(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		return actor.carryComponent.IsPOICarried(poiTarget);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, target, otherData, job))
		{
			return actor.homeSettlement != null;
		}
		return false;
	}

	private BedClinic GetValidBedForActor(Character actor, Character targetCharacter)
	{
		if (actor.homeSettlement != null)
		{
			List<LocationStructure> structuresOfType = actor.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.HOSPICE);
			if (structuresOfType != null)
			{
				for (int i = 0; i < structuresOfType.Count; i++)
				{
					BedClinic firstBedClinicThatCanBeUsedBy = structuresOfType[i].GetFirstBedClinicThatCanBeUsedBy(targetCharacter);
					if (firstBedClinicThatCanBeUsedBy != null)
					{
						return firstBedClinicThatCanBeUsedBy;
					}
				}
			}
		}
		return null;
	}

	private BedClinic GetBedNearActor(Character actor, Character targetCharacter)
	{
		BedClinic bedClinic = actor.gridTileLocation.tileObjectComponent.objHere as BedClinic;
		if (bedClinic == null || !bedClinic.IsAvailable() || !bedClinic.CanUseBed(targetCharacter))
		{
			for (int i = 0; i < actor.gridTileLocation.neighbourList.Count; i++)
			{
				if (actor.gridTileLocation.neighbourList[i].tileObjectComponent.objHere is BedClinic bedClinic2 && bedClinic2.IsAvailable() && bedClinic2.CanUseBed(targetCharacter))
				{
					bedClinic = bedClinic2;
					break;
				}
			}
		}
		return bedClinic;
	}

	public void PreQuarantineSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		Character character = goapNode.poiTarget as Character;
		BedClinic bedNearActor = GetBedNearActor(actor, character);
		goapNode.actor.UncarryPOI(character, bringBackToInventory: false, addToLocation: true, bedNearActor.gridTileLocation);
		int overrideDuration = -1;
		if (character.traitContainer.HasTrait("Plagued"))
		{
			overrideDuration = 0;
		}
		character.traitContainer.AddTrait(character, "Quarantined", null, bypassElementalChance: false, overrideDuration);
		bedNearActor.OnDoActionToObject(goapNode);
		character.jobQueue.CancelAllJobs("Quarantined");
	}
}
