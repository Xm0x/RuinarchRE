using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class AssumptionComponent : CharacterComponent
{
	public List<AssumptionData> assumptionData { get; private set; }

	public AssumptionComponent()
	{
		assumptionData = new List<AssumptionData>();
	}

	public AssumptionComponent(SaveDataAssumptionComponent data)
	{
		assumptionData = data.assumptionData;
	}

	public void CreateAndReactToNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType, REACTION_STATUS reactionStatus, bool isFabricated)
	{
		if (HasAlreadyAssumedTo(assumedActionType, assumedCharacter, targetOfAssumedCharacter) || !base.owner.limiterComponent.canWitness)
		{
			return;
		}
		assumptionData.Add(new AssumptionData(assumedActionType, assumedCharacter, targetOfAssumedCharacter));
		Assumption assumption = CreateNewAssumption(assumedCharacter, targetOfAssumedCharacter, assumedActionType, isFabricated);
		assumption.assumedAction.SetCrimeType();
		if (assumedActionType == INTERACTION_TYPE.ASSAULT)
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", "Abduct");
			if (LocalizationManager.Instance.HasLocalizedValue(localizedValue))
			{
				assumption.assumedAction.descriptionLog.AddToFillers(null, localizedValue, LOG_IDENTIFIER.STRING_1);
			}
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "assumed_event", LOG_TAG.Social, assumption.assumedAction);
		switch (reactionStatus)
		{
		case REACTION_STATUS.INFORMED:
			log.AddTag(LOG_TAG.Informed);
			break;
		case REACTION_STATUS.WITNESSED:
			log.AddTag(LOG_TAG.Witnessed);
			break;
		}
		if (assumption.assumedAction.crimeType != CRIME_TYPE.None)
		{
			log.AddTag(LOG_TAG.Crimes);
		}
		log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.PARTY_1);
		log.AddToFillers(null, assumption.informationLog.unreplacedText, LOG_IDENTIFIER.APPEND);
		log.AddToFillers(assumption.informationLog.fillers);
		log.AddLogToDatabase();
		assumption.SetAssumptionLog(log);
		if (PlayerManager.Instance.player.ShouldShowNotificationFrom(base.owner))
		{
			PlayerManager.Instance.player.ShowNotificationFrom(base.owner, InteractionManager.Instance.CreateNewIntel(assumption.assumedAction));
		}
		base.owner.reactionComponent.ReactTo(assumption, reactionStatus, addLog: false);
		if (targetOfAssumedCharacter is TileObject tileObject)
		{
			tileObject.AddCharacterThatAlreadyAssumed(base.owner);
		}
		Messenger.Broadcast(JobSignals.CHARACTER_ASSUMED, base.owner, assumedCharacter, targetOfAssumedCharacter);
	}

	private Assumption CreateNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType, bool isFabricated)
	{
		ActualGoapNode actualGoapNode = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[assumedActionType], assumedCharacter, targetOfAssumedCharacter, null, 0);
		Assumption assumption = new Assumption(base.owner, assumedCharacter);
		actualGoapNode.SetAsAssumption(assumption);
		actualGoapNode.SetIsFabricated(isFabricated);
		return assumption;
	}

	public bool HasAlreadyAssumedTo(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target)
	{
		for (int i = 0; i < this.assumptionData.Count; i++)
		{
			AssumptionData assumptionData = this.assumptionData[i];
			if (assumptionData.assumedActionType == actionType && assumptionData.actorID == actor.persistentID && assumptionData.targetID == target.persistentID && assumptionData.targetPOIType == target.poiType)
			{
				return true;
			}
		}
		return false;
	}

	public void LoadReferences(SaveDataAssumptionComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure pStructure)
	{
	}
}
