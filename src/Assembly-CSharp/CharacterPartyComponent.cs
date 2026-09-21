using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterPartyComponent : CharacterComponent
{
	public Party currentParty { get; private set; }

	public bool isFollowingBeacon { get; private set; }

	public bool hasParty => currentParty != null;

	public bool isActiveMember => IsPartyActiveAndOwnerActivePartOfQuest();

	public bool isMemberThatJoinedQuest => IsPartyActiveAndOwnerJoinedQuest();

	public CharacterPartyComponent()
	{
	}

	public CharacterPartyComponent(SaveDataCharacterPartyComponent data)
	{
	}

	public void SetCurrentParty(Party party)
	{
		currentParty = party;
	}

	private bool IsPartyActiveAndOwnerActivePartOfQuest()
	{
		if (hasParty && currentParty.isActive)
		{
			if (currentParty.DidMemberJoinQuest(base.owner))
			{
				return currentParty.IsMemberActive(base.owner);
			}
			return false;
		}
		return false;
	}

	private bool IsPartyActiveAndOwnerJoinedQuest()
	{
		if (hasParty && currentParty.isActive)
		{
			return currentParty.DidMemberJoinQuest(base.owner);
		}
		return false;
	}

	public bool IsAMemberOfParty(Party party)
	{
		if (currentParty != null)
		{
			return currentParty.IsPartyTheSameAsThisParty(party);
		}
		return false;
	}

	public void FollowBeacon()
	{
		if (!isFollowingBeacon)
		{
			if (hasParty)
			{
				Character currentBeaconCharacter = currentParty.beaconComponent.currentBeaconCharacter;
				if (currentBeaconCharacter != null && base.owner.hasMarker)
				{
					base.owner.marker.GoToPOI(currentBeaconCharacter, null, OnArriveFollowingBeacon);
					isFollowingBeacon = true;
					base.owner.movementComponent.UpdateSpeed();
				}
			}
		}
		else
		{
			UpdateFollowBeacon();
		}
	}

	private void OnArriveFollowingBeacon()
	{
		UnfollowBeacon();
	}

	private void UpdateFollowBeacon()
	{
		if (hasParty)
		{
			Character currentBeaconCharacter = currentParty.beaconComponent.currentBeaconCharacter;
			if (currentBeaconCharacter != null && base.owner.hasMarker)
			{
				base.owner.marker.GoToPOI(currentBeaconCharacter, null, OnArriveFollowingBeacon);
			}
		}
	}

	public void UnfollowBeacon()
	{
		if (isFollowingBeacon && hasParty && base.owner.hasMarker)
		{
			isFollowingBeacon = false;
			base.owner.movementComponent.UpdateSpeed();
			base.owner.marker.pathfindingAI.ClearAllCurrentPathData();
			base.owner.marker.StopMovement();
		}
	}

	public bool CanFollowBeacon()
	{
		Character currentBeaconCharacter = currentParty.beaconComponent.currentBeaconCharacter;
		if (hasParty && currentBeaconCharacter != null && base.owner != currentBeaconCharacter && base.owner.limiterComponent.canMove && base.owner.limiterComponent.canPerform && !base.owner.isDead)
		{
			if (isFollowingBeacon)
			{
				return true;
			}
			if (base.owner.stateComponent.currentState == null && (base.owner.currentActionNode == null || base.owner.currentActionNode.associatedJobType == JOB_TYPE.PARTY_GO_TO) && base.owner.hasMarker && !base.owner.marker.IsPOIInVision(currentBeaconCharacter))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasReachablePartymateToFleeTo()
	{
		LocationGridTile gridTileLocation = base.owner.gridTileLocation;
		if (isMemberThatJoinedQuest && gridTileLocation != null)
		{
			for (int i = 0; i < currentParty.membersThatJoinedQuest.Count; i++)
			{
				Character character = currentParty.membersThatJoinedQuest[i];
				LocationGridTile gridTileLocation2 = character.gridTileLocation;
				if (base.owner != character && character.limiterComponent.canPerform && character.limiterComponent.canMove && character.hasMarker && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried() && gridTileLocation2 != null && base.owner.movementComponent.HasPathToEvenIfDiffRegion(gridTileLocation2) && gridTileLocation.GetDistanceTo(gridTileLocation2) <= 20f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasPartymateInVision()
	{
		if (base.owner.hasMarker && isMemberThatJoinedQuest)
		{
			for (int i = 0; i < base.owner.marker.inVisionCharacters.Count; i++)
			{
				Character character = base.owner.marker.inVisionCharacters[i];
				if (character.partyComponent.IsAMemberOfParty(base.owner.partyComponent.currentParty) && character.partyComponent.isMemberThatJoinedQuest)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void LoadReferences(SaveDataCharacterPartyComponent data)
	{
		if (!string.IsNullOrEmpty(data.currentParty))
		{
			currentParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.currentParty);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
