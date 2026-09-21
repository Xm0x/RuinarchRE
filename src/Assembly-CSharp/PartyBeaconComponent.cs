using Inner_Maps.Location_Structures;

public class PartyBeaconComponent : PartyComponent
{
	public Character currentBeaconCharacter { get; private set; }

	public void Initialize()
	{
		SubscribeToSignals();
	}

	public void Initialize(SaveDataPartyBeaconComponent data)
	{
		SubscribeToSignals();
	}

	private void SubscribeToSignals()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCannotMove);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCannotPerform);
		Messenger.AddListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
	}

	private void UnsubscribeFromSignals()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCannotMove);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCannotPerform);
		Messenger.RemoveListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
		Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
	}

	private void OnCharacterCannotMove(Character p_character)
	{
		if (currentBeaconCharacter == p_character)
		{
			UpdateBeaconCharacter();
		}
	}

	private void OnCharacterCannotPerform(Character p_character)
	{
		if (currentBeaconCharacter == p_character)
		{
			UpdateBeaconCharacter();
		}
	}

	private void OnSeizePOI(IPointOfInterest p_poi)
	{
		if (currentBeaconCharacter == p_poi)
		{
			UpdateBeaconCharacter();
		}
	}

	private void OnCharacterRemovedFromVision(Character p_character, Character p_target)
	{
		if (p_target == currentBeaconCharacter && p_character.partyComponent.IsAMemberOfParty(base.owner) && base.owner.partyState == PARTY_STATE.Moving && p_character.partyComponent.isMemberThatJoinedQuest && p_character.partyComponent.CanFollowBeacon())
		{
			p_character.partyComponent.FollowBeacon();
		}
	}

	private void SetBeaconCharacter(Character p_character)
	{
		if (currentBeaconCharacter != p_character)
		{
			currentBeaconCharacter = p_character;
			UpdateMovementOfAllMembersAccordingToBeacon();
		}
	}

	public void UpdateBeaconCharacter()
	{
		if (!base.owner.isPlayerParty)
		{
			return;
		}
		if (base.owner.partyState == PARTY_STATE.Moving)
		{
			bool flag = false;
			for (int i = 0; i < base.owner.membersThatJoinedQuest.Count; i++)
			{
				Character character = base.owner.membersThatJoinedQuest[i];
				if (!character.isBeingSeized && character.limiterComponent.canPerform && character.limiterComponent.canMove && !character.isDead && base.owner.IsMemberActive(character))
				{
					flag = true;
					SetBeaconCharacter(character);
					break;
				}
			}
			if (!flag)
			{
				SetBeaconCharacter(null);
			}
		}
		else
		{
			SetBeaconCharacter(null);
		}
	}

	public void UpdateMovementOfAllMembersAccordingToBeacon()
	{
		if (currentBeaconCharacter != null)
		{
			if (base.owner.partyState != PARTY_STATE.Moving)
			{
				return;
			}
			for (int i = 0; i < base.owner.membersThatJoinedQuest.Count; i++)
			{
				Character character = base.owner.membersThatJoinedQuest[i];
				if (character.partyComponent.CanFollowBeacon())
				{
					character.partyComponent.FollowBeacon();
				}
				else
				{
					character.partyComponent.UnfollowBeacon();
				}
			}
		}
		else
		{
			for (int j = 0; j < base.owner.membersThatJoinedQuest.Count; j++)
			{
				base.owner.membersThatJoinedQuest[j].partyComponent.UnfollowBeacon();
			}
		}
	}

	public void OnDestroyParty()
	{
		UnsubscribeFromSignals();
		SetBeaconCharacter(null);
	}

	public void OnRemoveMemberThatJoinedQuest(Character p_member)
	{
		if (p_member == currentBeaconCharacter)
		{
			UpdateBeaconCharacter();
		}
		p_member.partyComponent.UnfollowBeacon();
	}

	public void LoadReferences(SaveDataPartyBeaconComponent data)
	{
		if (!string.IsNullOrEmpty(data.currentBeaconCharacter))
		{
			currentBeaconCharacter = CharacterManager.Instance.GetCharacterByPersistentID(data.currentBeaconCharacter);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (currentBeaconCharacter == p_character)
		{
			SetBeaconCharacter(null);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = currentBeaconCharacter;
	}
}
