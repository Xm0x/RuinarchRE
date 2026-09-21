using Inner_Maps.Location_Structures;
using Quests.Alerts;
using Tutorial;

public class DevastationRitual : GoapAction
{
	public override bool shouldShowNotifRegardlessOfWatcher => true;

	public DevastationRitual()
		: base(INTERACTION_TYPE.DEVASTATION_RITUAL)
	{
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.showNotification = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Ritual Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		LocationStructure locationStructure = ((node.otherData == null || node.otherData.Length != 1 || !(node.otherData[0].obj is LocationStructure locationStructure2)) ? node.poiTarget.gridTileLocation?.structure : locationStructure2);
		if (locationStructure != null && locationStructure.isProtected)
		{
			node.actor.limiterComponent.IncreaseCanWitness();
			locationStructure.SetIsProtected(p_state: false);
		}
	}

	public override void OnStopWhileStarted(ActualGoapNode node)
	{
		base.OnStopWhileStarted(node);
		LocationStructure locationStructure = ((node.otherData == null || node.otherData.Length != 1 || !(node.otherData[0].obj is LocationStructure locationStructure2)) ? node.poiTarget.gridTileLocation?.structure : locationStructure2);
		if (locationStructure != null && locationStructure.isProtected)
		{
			node.actor.limiterComponent.IncreaseCanWitness();
			locationStructure.SetIsProtected(p_state: false);
		}
	}

	public void PreRitualSuccess(ActualGoapNode goapNode)
	{
		LocationStructure locationStructure = ((goapNode.otherData == null || goapNode.otherData.Length != 1 || !(goapNode.otherData[0].obj is LocationStructure locationStructure2)) ? goapNode.poiTarget.gridTileLocation?.structure : locationStructure2);
		if (locationStructure != null && !locationStructure.isProtected)
		{
			goapNode.actor.limiterComponent.DecreaseCanWitness();
			locationStructure.SetIsProtected(p_state: true, goapNode.actor);
		}
		DevastationRitualAlert devastationRitualAlert = TutorialManager.Instance.CreateGameAlert<DevastationRitualAlert>(Game_Alert.Devastation_Ritual_Alert);
		devastationRitualAlert.SetDevastationRitualActor(goapNode.actor);
		devastationRitualAlert.SetAsActive();
	}

	public void AfterRitualSuccess(ActualGoapNode goapNode)
	{
		LocationStructure locationStructure = ((goapNode.otherData == null || goapNode.otherData.Length != 1 || !(goapNode.otherData[0].obj is LocationStructure locationStructure2)) ? goapNode.poiTarget.gridTileLocation?.structure : locationStructure2);
		if (locationStructure != null && locationStructure.isProtected)
		{
			goapNode.actor.limiterComponent.IncreaseCanWitness();
			locationStructure.SetIsProtected(p_state: false);
		}
		int count = PlayerManager.Instance.player.playerSettlement.allStructures.Count;
		PlayerManager.Instance.player.devastationComponent.SetHasMeteorDevastation(p_state: true, count);
		goapNode.actor.classComponent.ResetDevastationCounter();
		Messenger.Broadcast(CharacterSignals.CHARACTER_FINISHED_DEVASTATION_RITUAL, goapNode.actor);
	}
}
