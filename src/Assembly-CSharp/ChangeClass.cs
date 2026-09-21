using Inner_Maps.Location_Structures;

public class ChangeClass : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public ChangeClass()
		: base(INTERACTION_TYPE.CHANGE_CLASS)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Change Class Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 1;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		Character actor = node.actor;
		StringOtherData stringOtherData = node.otherData[0] as StringOtherData;
		if (stringOtherData.str == "Combatant")
		{
			if (actor.faction != null && actor.faction.ideologyComponent.HasIdeology(FACTION_IDEOLOGY.Mage_Guild))
			{
				stringOtherData.SetString("Mage");
			}
			else
			{
				stringOtherData.SetString(node.actor.classComponent.GetRandomHighestAbleCombatantClass());
			}
		}
		string displayName = CharacterManager.Instance.GetCharacterClass(stringOtherData.str).displayName;
		log.AddToFillers(null, displayName, LOG_IDENTIFIER.STRING_1);
	}

	public void AfterChangeClassSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		string text = (string)otherData[0].obj;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		goapNode.actor.classComponent.AssignClass(text);
		goapNode.actor.classComponent.SetShouldChangeClass(p_state: false);
		if (otherData.Length > 1)
		{
			OtherData otherData2 = otherData[1];
			if (otherData2 != null && otherData2.obj is ManMadeStructure { hasBeenDestroyed: false } manMadeStructure && manMadeStructure.AddAssignedWorker(goapNode.actor))
			{
				goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Claim_Work_Structure, goapNode.actor);
			}
		}
	}
}
