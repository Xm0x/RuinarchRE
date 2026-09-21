using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class Rumor : IReactable
{
	public Character characterThatCreatedRumor { get; private set; }

	public Character targetCharacter { get; private set; }

	public IRumorable rumorable { get; private set; }

	public string name => rumorable.name;

	public string classificationName => "Rumor";

	public Character actor => rumorable.actor;

	public IPointOfInterest target => rumorable.target;

	public Character disguisedActor => rumorable.disguisedActor;

	public Character disguisedTarget => rumorable.disguisedTarget;

	public Log informationLog => rumorable.informationLog;

	public bool isStealth => rumorable.isStealth;

	public CRIME_TYPE crimeType => rumorable.crimeType;

	public List<Character> awareCharacters => rumorable.awareCharacters;

	public List<LOG_TAG> logTags => rumorable.logTags;

	public bool isIntel => rumorable.isStealth;

	public Rumor(Character characterThatCreated, Character targetCharacter)
	{
		characterThatCreatedRumor = characterThatCreated;
		this.targetCharacter = targetCharacter;
	}

	public void SetRumorable(IRumorable rumorable)
	{
		this.rumorable = rumorable;
	}

	public string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return rumorable.ReactionToActor(actor, target, witness, status);
	}

	public string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		return rumorable.ReactionToTarget(actor, target, witness, status);
	}

	public string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		return rumorable.ReactionOfTarget(actor, target, status);
	}

	public void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		rumorable.PopulateReactionsToActor(reactions, actor, target, witness, status);
	}

	public void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status)
	{
		rumorable.PopulateReactionsToTarget(reactions, actor, target, witness, status);
	}

	public void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status)
	{
		rumorable.PopulateReactionsOfTarget(reactions, actor, target, status);
	}

	public REACTABLE_EFFECT GetReactableEffect(Character witness)
	{
		return REACTABLE_EFFECT.Negative;
	}

	public void AddAwareCharacter(Character character)
	{
		rumorable.AddAwareCharacter(character);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		IsCharacterReferenced(p_character);
	}

	public bool IsStructureReferenced(LocationStructure p_structure)
	{
		return false;
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		if (characterThatCreatedRumor == p_character)
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
		if (characterThatCreatedRumor == p_character)
		{
			characterThatCreatedRumor = null;
		}
		if (targetCharacter == p_character)
		{
			targetCharacter = null;
		}
	}
}
