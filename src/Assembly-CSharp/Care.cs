using Traits;
using UnityEngine;

public class Care : GoapAction
{
	public Care()
		: base(INTERACTION_TYPE.CARE)
	{
		base.actionIconString = GoapActionStateDB.FirstAid_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Care Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public void PerTickCareSuccess(ActualGoapNode goapNode)
	{
		Plagued traitOrStatus = goapNode.target.traitContainer.GetTraitOrStatus<Plagued>("Plagued");
		if (traitOrStatus == null)
		{
			return;
		}
		GameDate latestExpiryDate = goapNode.target.traitContainer.GetLatestExpiryDate(traitOrStatus.name);
		if (latestExpiryDate.hasValue && GameManager.Instance.Today().GetTickDifference(latestExpiryDate) > 20)
		{
			GameDate p_newRemoveDate = latestExpiryDate;
			p_newRemoveDate.ReduceTicks(20 * Random.Range(1, 3));
			if (p_newRemoveDate.IsBefore(GameManager.Instance.Today()))
			{
				p_newRemoveDate = GameManager.Instance.Today();
				p_newRemoveDate.AddTicks(1);
			}
			goapNode.target.traitContainer.RescheduleLatestTraitRemoval(goapNode.target, traitOrStatus, p_newRemoveDate);
		}
	}

	public void AfterCareSuccess(ActualGoapNode goapNode)
	{
		goapNode.target.traitContainer.AddTrait(goapNode.target, "Plague Cared", goapNode.actor);
		goapNode.actor.traitContainer.GetTraitOrStatus<Trait>("Plague Cared")?.SetGainedFromDoingAction(goapNode.action.goapType, goapNode.isStealth);
	}
}
