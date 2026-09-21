public interface IIntel
{
	IReactable reactable { get; }

	Log log { get; }

	Character actor { get; }

	IPointOfInterest target { get; }

	string GetIntelInfoRelationshipText();

	string GetIntelInfoBlackmailText();

	void OnIntelRemoved();

	bool CanBeUsedToBlackmailCharacter(Character p_target);

	bool IsIntelConsideredACrimeByTarget(Character p_target);

	BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(Character p_target);

	string GetFullIntelTooltip();

	bool CanShareIntelTo(Character p_target);
}
