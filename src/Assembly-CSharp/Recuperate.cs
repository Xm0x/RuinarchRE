using Inner_Maps.Location_Structures;

public class Recuperate : GoapAction
{
	public Recuperate()
		: base(INTERACTION_TYPE.RECUPERATE)
	{
		base.actionIconString = GoapActionStateDB.FirstAid_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Recuperate Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnStopWhilePerforming(ActualGoapNode p_node)
	{
		base.OnStopWhilePerforming(p_node);
		p_node.actor.traitContainer.RemoveTrait(p_node.actor, "Recuperating");
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		node.actor.traitContainer.RemoveTrait(node.actor, "Recuperating");
	}

	private bool IsSubjectForRecuperate(ActualGoapNode p_node)
	{
		return p_node.actor.traitContainer.HasTrait("Injured", "Plagued", "Poisoned", "Burnt");
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public void PreRecuperateSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Recuperating");
	}

	public void AfterRecuperateSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Recuperating");
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Poisoned");
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Plagued");
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Injured");
		goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Burnt");
		LocationStructure locationStructure = goapNode.poiTarget.gridTileLocation?.structure;
		if (locationStructure != null && locationStructure.structureType == STRUCTURE_TYPE.HOSPICE && locationStructure is ManMadeStructure manMadeStructure && manMadeStructure.HasAssignedWorker())
		{
			string id = manMadeStructure.assignedWorkerIDs[0];
			DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id).moneyComponent.AdjustCoins(28);
		}
	}

	public void PerTickRecuperateSuccess(ActualGoapNode p_node)
	{
		CharacterNeedsComponent needsComponent = p_node.actor.needsComponent;
		if (needsComponent.HasNeeds())
		{
			needsComponent.AdjustTiredness(0.417f);
		}
		if (!IsSubjectForRecuperate(p_node))
		{
			p_node.associatedJob.CancelJob();
		}
	}
}
