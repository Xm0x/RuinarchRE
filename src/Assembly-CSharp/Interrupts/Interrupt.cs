using System.Collections.Generic;
using Crime_System;
using Inner_Maps;
using Object_Pools;
using UtilityScripts;

namespace Interrupts;

public class Interrupt
{
	public INTERRUPT type { get; protected set; }

	public string name { get; protected set; }

	public int duration { get; protected set; }

	public bool isSimulateneous { get; protected set; }

	public bool doesStopCurrentAction { get; protected set; }

	public bool doesDropCurrentJob { get; protected set; }

	public string interruptIconString { get; protected set; }

	public bool isIntel { get; protected set; }

	public bool shouldAddLogs { get; protected set; }

	public bool shouldShowNotif { get; protected set; }

	public bool shouldEndOnSeize { get; protected set; }

	public bool shouldStopMovement { get; protected set; }

	public LOG_TAG[] logTags { get; protected set; }

	protected Interrupt(INTERRUPT type)
	{
		this.type = type;
		name = Utilities.NotNormalizedConversionEnumToString(type.ToStringEnum());
		isSimulateneous = false;
		interruptIconString = GoapActionStateDB.No_Icon;
		shouldAddLogs = true;
		shouldShowNotif = true;
		shouldStopMovement = true;
	}

	public virtual bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		return false;
	}

	public virtual bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		return false;
	}

	public virtual bool OnForceEndInterrupt(InterruptHolder interruptHolder)
	{
		return false;
	}

	public virtual bool PerTickInterrupt(InterruptHolder interruptHolder)
	{
		return false;
	}

	public virtual string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		if ((interrupt.interrupt.type == INTERRUPT.Transform_To_Wolf || interrupt.interrupt.type == INTERRUPT.Revert_To_Normal) && actor.isLycanthrope)
		{
			actor = actor.lycanData.originalForm;
		}
		string empty = string.Empty;
		bool flag;
		if (status == REACTION_STATUS.WITNESSED)
		{
			flag = witness.IsHostileWith(actor);
		}
		else
		{
			flag = witness.IsHostileWithCheckingForInformed(actor);
			if (flag && CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, interrupt.crimeType).IsConsideredACrime() && target is Character character && !witness.IsHostileWithCheckingForInformed(character) && witness.relationshipContainer.IsFriendsWith(character))
			{
				LocationGridTile gridTileLocation = witness.gridTileLocation;
				LocationGridTile gridTileLocation2 = actor.gridTileLocation;
				if (gridTileLocation != null && gridTileLocation2 != null && gridTileLocation.area.GetAreaDistanceTo(gridTileLocation2.area) <= 3 && witness.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation2))
				{
					witness.combatComponent.Fight(actor, "Slay_Target");
					return "Attack_Nearby_Actor";
				}
			}
		}
		if (flag)
		{
			CrimeType crimeType = CrimeManager.Instance.GetCrimeType(interrupt.crimeType);
			if (crimeType != null)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "hostile_crime_reaction_with_crime", LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major);
				log.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddToFillers(null, crimeType.name, LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(witness, log);
				LogPool.Release(log);
			}
			else
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterCrimeSystem_Table", "hostile_crime_reaction", LOG_TAG.Life_Changes, LOG_TAG.Crimes, LOG_TAG.Major);
				log2.AddToFillers(witness, witness.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFrom(witness, log2);
				LogPool.Release(log2);
			}
			return "Stop_Telling_News";
		}
		interrupt.IncreaseReactionCounter();
		empty = CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, interrupt.crimeType, interrupt, status);
		interrupt.DecreaseReactionCounter();
		string text = string.Empty;
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		PopulateReactionsToActor(list, actor, target, witness, interrupt, status);
		int p_totalOpinionReduction = 0;
		string p_lastStrawReasonKey = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			text += CharacterManager.Instance.TriggerEmotion(list[i], witness, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, null, "", p_triggerOpinionChangesEffect: false);
		}
		if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
		{
			witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
		}
		RuinarchListPool<EMOTION>.Release(list);
		if (string.IsNullOrEmpty(empty))
		{
			empty += text;
		}
		return empty;
	}

	public virtual string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
		string text = string.Empty;
		if (target is Character character)
		{
			if ((status != REACTION_STATUS.WITNESSED) ? witness.IsHostileWithCheckingForInformed(character) : witness.IsHostileWith(character))
			{
				return text;
			}
			List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
			PopulateReactionsToTarget(list, actor, character, witness, interrupt, status);
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			for (int i = 0; i < list.Count; i++)
			{
				text += CharacterManager.Instance.TriggerEmotion(list[i], witness, character, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, null, "", p_triggerOpinionChangesEffect: false);
			}
			if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !character.reactionComponent.isDisguised)
			{
				witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, character, p_lastStrawReasonKey, p_totalOpinionReduction);
			}
			RuinarchListPool<EMOTION>.Release(list);
		}
		return text;
	}

	public virtual string ReactionOfTarget(Character actor, IPointOfInterest target, InterruptHolder interrupt, REACTION_STATUS status)
	{
		if (target is Character character)
		{
			List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
			PopulateReactionsOfTarget(list, actor, target, interrupt, status);
			string text = string.Empty;
			int p_totalOpinionReduction = 0;
			string p_lastStrawReasonKey = string.Empty;
			for (int i = 0; i < list.Count; i++)
			{
				text += CharacterManager.Instance.TriggerEmotion(list[i], character, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, null, "", p_triggerOpinionChangesEffect: false);
			}
			if (p_totalOpinionReduction < 0 && !character.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
			{
				character.relationshipContainer.CreateJobsOnOpinionReduced(character, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
			}
			RuinarchListPool<EMOTION>.Release(list);
			return text;
		}
		return string.Empty;
	}

	public virtual void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
	}

	public virtual void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status)
	{
	}

	public virtual void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, InterruptHolder interrupt, REACTION_STATUS status)
	{
	}

	public virtual Log CreateEffectLog(Character actor, IPointOfInterest target)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("Interrupts_Table", name + " effect"))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", name + " effect", logTags);
			if (isIntel)
			{
				log.AddTag(LOG_TAG.Intel);
			}
			if (actor != null)
			{
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			}
			if (target != null)
			{
				log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			}
			return log;
		}
		return null;
	}

	public virtual Log CreateEffectLog(Character actor, IPointOfInterest target, string key)
	{
		if (LocalizationManager.Instance.HasLocalizedValue("Interrupts_Table", name + " " + key))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", name + " " + key, logTags);
			if (isIntel)
			{
				log.AddTag(LOG_TAG.Intel);
			}
			if (actor != null)
			{
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			}
			if (target != null)
			{
				log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			}
			return log;
		}
		return null;
	}

	public virtual void AddAdditionalFillersToThoughtLog(Log log, Character actor)
	{
	}

	public virtual CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, InterruptHolder crime)
	{
		return CRIME_TYPE.None;
	}

	public virtual bool ShouldAddLogs(InterruptHolder interruptHolder)
	{
		return shouldAddLogs;
	}
}
