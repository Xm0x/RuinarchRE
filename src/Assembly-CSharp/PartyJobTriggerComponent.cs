using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class PartyJobTriggerComponent : JobTriggerComponent
{
	private readonly Party _owner;

	public PartyJobTriggerComponent(Party owner)
	{
		_owner = owner;
	}

	public bool CreateBuildCampfireJob(JOB_TYPE jobType)
	{
		if (!_owner.jobBoard.HasJob(jobType))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUILD_CAMPFIRE, null, _owner);
			goapPlanJob.SetDoNotRecalculate(state: true);
			_owner.jobBoard.AddToAvailableJobs(goapPlanJob);
			return true;
		}
		return false;
	}

	public void CreateHaulForCampJob(ResourcePile target, Area p_area)
	{
		if (!_owner.jobBoard.HasJob(JOB_TYPE.HAUL, target) && target.gridTileLocation.area != p_area)
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[1] { p_area });
			goapPlanJob.SetDoNotRecalculate(state: true);
			_owner.jobBoard.AddToAvailableJobs(goapPlanJob);
		}
	}

	public bool CreateSnatchJob(Character targetCharacter, LocationGridTile targetLocation, LocationStructure structure)
	{
		if (!_owner.jobBoard.HasJob(JOB_TYPE.SNATCH, targetCharacter))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SNATCH, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeSnatchJob");
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[2] { structure, targetLocation });
			_owner.jobBoard.AddToAvailableJobs(goapPlanJob);
			return true;
		}
		return false;
	}

	public bool CreateStealJob(TileObject targetItem, LocationStructure structure)
	{
		if (!_owner.jobBoard.HasJob(JOB_TYPE.DEMON_STEAL, targetItem))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_STEAL, INTERACTION_TYPE.DROP_ITEM, targetItem, _owner);
			goapPlanJob.SetCanTakeThisJobChecker("CanTakeStealJob");
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[1] { structure });
			_owner.jobBoard.AddToAvailableJobs(goapPlanJob);
			return true;
		}
		return false;
	}

	public bool TryCreateApprehend(Character actor, Character target, Party party, LocationStructure intendedPrison, ref bool canDoJob, out JobQueueItem producedJob)
	{
		producedJob = null;
		LocationStructure locationStructure = intendedPrison;
		Prisoner traitOrStatus = target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
		if (locationStructure == null)
		{
			locationStructure = ((traitOrStatus == null) ? actor.GetSettlementPrisonFor(target) : traitOrStatus.GetIntendedPrisonAccordingTo(actor));
		}
		bool flag = target.traitContainer.HasTrait("Criminal") && target.crimeComponent.IsWantedBy(actor.faction);
		bool flag2 = traitOrStatus?.IsConsideredPrisonerOf(actor) ?? false;
		canDoJob = locationStructure != null && target.currentStructure != locationStructure && actor.homeSettlement != null && (flag || flag2);
		if (canDoJob && !party.jobBoard.HasJob(JOB_TYPE.APPREHEND, target))
		{
			GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP_RESTRAINED, target, actor);
			goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { locationStructure });
			producedJob = goapPlanJob;
			return true;
		}
		return false;
	}

	public bool CreateKillJob(Character targetCharacter)
	{
		if (!_owner.jobBoard.HasJob(JOB_TYPE.DEMON_KILL, targetCharacter))
		{
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_KILL, INTERACTION_TYPE.ASSAULT, targetCharacter, _owner);
			_owner.jobBoard.AddToAvailableJobs(job);
			return true;
		}
		return false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
