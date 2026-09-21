using System.Collections.Generic;

public interface IReactable
{
	string name { get; }

	string classificationName { get; }

	Character actor { get; }

	IPointOfInterest target { get; }

	Character disguisedActor { get; }

	Character disguisedTarget { get; }

	Log informationLog { get; }

	bool isStealth { get; }

	bool isIntel { get; }

	CRIME_TYPE crimeType { get; }

	List<Character> awareCharacters { get; }

	List<LOG_TAG> logTags { get; }

	string ReactionToActor(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);

	string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);

	string ReactionOfTarget(Character actor, IPointOfInterest target, REACTION_STATUS status);

	void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);

	void PopulateReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, REACTION_STATUS status);

	void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, REACTION_STATUS status);

	void AddAwareCharacter(Character character);

	REACTABLE_EFFECT GetReactableEffect(Character witness);
}
