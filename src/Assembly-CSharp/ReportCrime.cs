using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

public class ReportCrime : GoapAction
{
	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;

	public ReportCrime()
		: base(INTERACTION_TYPE.REPORT_CRIME)
	{
		base.actionIconString = GoapActionStateDB.Report_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Crimes,
			LOG_TAG.Major
		};
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Report Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		_ = node.actor;
		_ = node.poiTarget;
		OtherData[] otherData = node.otherData;
		if (otherData.Length == 2 && otherData[0].obj is ICrimeable { actor: var character } crimeable)
		{
			if (crimeable.disguisedActor != null)
			{
				character = crimeable.disguisedActor;
			}
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.CHARACTER_3);
		}
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			Character character = poiTarget as Character;
			if (!character.carryComponent.IsNotBeingCarried())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_carried";
			}
			else if (!character.limiterComponent.canWitness)
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_inactive";
			}
		}
		return goapActionInvalidity;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, poiTarget, witness, node, status);
		OtherData[] otherData = node.otherData;
		if (otherData[0].obj is ICrimeable crimeable && otherData[1].obj is CrimeData crimeData && status == REACTION_STATUS.INFORMED && crimeable.name != "Report Crime")
		{
			ProcessInformation(node.actor, witness, crimeable, crimeData, node);
		}
		return result;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (!(node.otherData[0].obj is ICrimeable crimeable))
		{
			return;
		}
		if (witness == crimeable.actor)
		{
			reactions.Add(EMOTION.Anger);
			if (witness.relationshipContainer.IsLoverOrAffair(actor) || witness.relationshipContainer.IsFriendsWith(actor))
			{
				reactions.Add(EMOTION.Betrayal);
			}
		}
		else if (witness.relationshipContainer.GetOpinionLabel(crimeable.actor) == "Close Friend" || witness.relationshipContainer.IsLoverOrAffair(crimeable.actor))
		{
			reactions.Add(EMOTION.Anger);
		}
		else if (witness.relationshipContainer.IsEnemiesWith(crimeable.actor))
		{
			reactions.Add(EMOTION.Approval);
		}
		else
		{
			reactions.Add(EMOTION.Concern);
		}
	}

	public void PreReportSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.otherData[0].obj is ICrimeable { actor: var actor } && actor.isDead)
		{
			Character actor2 = goapNode.actor;
			Character character = goapNode.poiTarget as Character;
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " dead_criminal", LOG_TAG.Crimes);
			log.AddToFillers(actor2, actor2.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.CHARACTER_3);
			goapNode.OverrideDescriptionLog(log);
		}
	}

	public void AfterReportSuccess(ActualGoapNode goapNode)
	{
		OtherData[] otherData = goapNode.otherData;
		ICrimeable crime = otherData[0].obj as ICrimeable;
		CrimeData crimeData = otherData[1].obj as CrimeData;
		Character actor = goapNode.actor;
		Character recipient = goapNode.poiTarget as Character;
		actor.crimeComponent.AddReportedCrime(crimeData);
		crimeData?.SetReporter(actor);
		ProcessInformation(actor, recipient, crime, crimeData, goapNode);
	}

	private void ProcessInformation(Character sharer, Character recipient, ICrimeable crime, CrimeData crimeData, ActualGoapNode shareActionItself)
	{
		if (crime == null)
		{
			DoNotBelieve(sharer, recipient, crime, shareActionItself, "Disbelief");
			return;
		}
		Character character = crime.actor;
		_ = crime.target;
		if (crime.disguisedActor != null)
		{
			character = crime.disguisedActor;
		}
		if (crime.disguisedTarget != null)
		{
			_ = crime.disguisedTarget;
		}
		if (character.isDead || character == recipient)
		{
			return;
		}
		WeightedDictionary<string> weightedDictionary = new WeightedDictionary<string>();
		int num = 150;
		int num2 = 50;
		string opinionLabel = recipient.relationshipContainer.GetOpinionLabel(sharer);
		string opinionLabel2 = recipient.relationshipContainer.GetOpinionLabel(character);
		if (sharer.traitContainer.HasTrait("Persuasive"))
		{
			num += 500;
		}
		switch (opinionLabel)
		{
		case "Friend":
			num += 100;
			break;
		case "Close Friend":
			num += 250;
			break;
		case "Enemy":
			num2 += 100;
			break;
		case "Rival":
			num2 += 250;
			break;
		}
		switch (crime.GetReactableEffect(recipient))
		{
		case REACTABLE_EFFECT.Positive:
			switch (opinionLabel2)
			{
			case "Friend":
			case "Close Friend":
				num += 500;
				break;
			case "Enemy":
				num2 += 250;
				break;
			case "Rival":
				num2 += 500;
				break;
			}
			break;
		case REACTABLE_EFFECT.Negative:
			switch (opinionLabel2)
			{
			case "Enemy":
			case "Rival":
				num += 250;
				break;
			case "Friend":
				num2 += 250;
				break;
			case "Close Friend":
				num2 += 500;
				break;
			}
			if (character.isSettlementRuler || character.isFactionLeader)
			{
				num2 += 350;
			}
			break;
		}
		weightedDictionary.AddElement("Belief", num);
		weightedDictionary.AddElement("Disbelief", num2);
		string text = weightedDictionary.PickRandomElementGivenWeights();
		if (text == "Belief")
		{
			CRIME_TYPE crimeType = crime.crimeType;
			CRIME_SEVERITY p_crimeSeverity = CRIME_SEVERITY.None;
			if (crimeType != CRIME_TYPE.Unset && crimeType != CRIME_TYPE.None)
			{
				p_crimeSeverity = CrimeManager.Instance.GetCrimeSeverity(recipient, crime.actor, crime.target, crimeType);
			}
			if (p_crimeSeverity.IsConsideredACrime())
			{
				recipient.reactionComponent.ReactTo(crime, REACTION_STATUS.INFORMED, addLog: false);
			}
			else
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " not_crime", LOG_TAG.Crimes);
				log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
		}
		else
		{
			DoNotBelieve(sharer, recipient, crime, shareActionItself, text);
		}
		if (recipient == shareActionItself.poiTarget)
		{
			crimeData.SetHasAuthoritiesReachedADecision(p_state: true);
		}
	}

	private void DoNotBelieve(Character sharer, Character recipient, ICrimeable crime, ActualGoapNode shareActionItself, string result)
	{
		CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, crime as ActualGoapNode);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", base.goapName + " " + result, base.logTags);
		log.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		string value = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Crimes").ToLower();
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFrom(sharer, log);
		LogPool.Release(log);
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			Character character = poiTarget as Character;
			if (actor != character)
			{
				return !GameUtilities.IsRaceBeast(character.race);
			}
			return false;
		}
		return false;
	}
}
