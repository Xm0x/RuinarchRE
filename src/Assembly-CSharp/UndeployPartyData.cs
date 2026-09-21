using Inner_Maps.Location_Structures;

public class UndeployPartyData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.UNDEPLOY_PARTY;

	public override string name => "Undeploy Party";

	public override string description => "Undeploy this character and its party.";

	public UndeployPartyData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			Messenger.Broadcast(PartySignals.UNDEPLOY_PLAYER_PARTY, character.partyComponent.currentParty);
		}
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		if (targetStructure.partyStructureComponent != null && targetStructure.partyStructureComponent.party != null)
		{
			Messenger.Broadcast(PartySignals.UNDEPLOY_PLAYER_PARTY, targetStructure.partyStructureComponent.party);
		}
		base.ActivateAbility(targetStructure);
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			return !targetCharacter.isDead;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (flag)
		{
			if (target is Character character)
			{
				if (character.partyComponent.currentParty == null)
				{
					return false;
				}
				if (character.partyComponent.currentParty == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty)
				{
					return false;
				}
				if (!character.partyComponent.currentParty.isPlayerParty && (character.partyComponent.currentParty.currentQuest == null || !character.partyComponent.currentParty.currentQuest.isDemonicQuest))
				{
					return false;
				}
				return true;
			}
			if (target is LocationStructure { partyStructureComponent: not null } locationStructure)
			{
				return locationStructure.partyStructureComponent.party != null;
			}
			if (target is TileObject tileObject)
			{
				return tileObject.partyComponent.currentSnatchObjectParty != null;
			}
			return true;
		}
		return flag;
	}
}
