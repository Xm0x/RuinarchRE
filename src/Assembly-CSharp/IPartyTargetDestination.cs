using Inner_Maps;

public interface IPartyTargetDestination
{
	string persistentID { get; }

	string name { get; }

	PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType { get; }

	bool hasBeenDestroyed { get; }

	Region region { get; }

	LocationGridTile GetRandomPassableTile();

	bool IsAtTargetDestination(Character character);
}
