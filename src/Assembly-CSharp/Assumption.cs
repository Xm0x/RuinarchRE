using System.Collections.Generic;
using Object_Pools;

public class Assumption : IReactable
{
	public Character characterThatCreatedAssumption { get; private set; }

	public Character targetCharacter { get; private set; }

	public ActualGoapNode assumedAction { get; private set; }

	public Log assumptionLog { get; private set; }

	public string name => assumedAction.name;

	public string classificationName => "Assumption";

	public Character actor => assumedAction.actor;

	public IPointOfInterest target => assumedAction.target;

	public Character disguisedActor => assumedAction.disguisedActor;

	public Character disguisedTarget => assumedAction.disguisedTarget;

	public Log informationLog => assumedAction.informationLog;

	public bool isStealth => assumedAction.isStealth;

	public CRIME_TYPE crimeType => assumedAction.crimeType;

	public List<Character> awareCharacters => assumedAction.awareCharacters;

	public List<LOG_TAG> logTags => assumedAction.logTags;

	public bool isIntel => assumedAction.isIntel;

	public Assumption(Character characterThatCreated, Character targetCharacter)
	{
		characterThatCreatedAssumption = characterThatCreated;
		this.targetCharacter = targetCharacter;
	}

	public void SetAssumedAction(ActualGoapNode assumedAction)
	{
		this.assumedAction = assumedAction;
	}

	public void SetAssumptionLog(Log p_log)
	{
		if (assumptionLog != null)
		{
			LogPool.Release(assumptionLog);
		}
		assumptionLog = p_log;
	}

	public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return assumedAction.ReactionToActor(actor, target, witness, status);
	}

	public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return assumedAction.ReactionToTarget(actor, target, witness, status);
	}

	public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		return assumedAction.ReactionOfTarget(actor, target, status);
	}

	public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		assumedAction.PopulateReactionsToActor(reactions, actor, target, witness, status);
	}

	public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		assumedAction.PopulateReactionsToTarget(reactions, actor, target, witness, status);
	}

	public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		assumedAction.PopulateReactionsOfTarget(reactions, actor, target, status);
	}

	public REACTABLE_EFFECT GetReactableEffect(Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public void AddAwareCharacter(Character character)
	{
		assumedAction.AddAwareCharacter(character);
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		if (characterThatCreatedAssumption == p_character)
		{
			return true;
		}
		if (targetCharacter == p_character)
		{
			return true;
		}
		return false;
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (characterThatCreatedAssumption == p_character)
		{
			characterThatCreatedAssumption = null;
		}
		if (targetCharacter == p_character)
		{
			targetCharacter = null;
		}
	}
}
