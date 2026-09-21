using System;
using Inner_Maps.Location_Structures;

public class CounterattackPartyQuest : PartyQuest
{
	public int numberOfStructureToBeDestroyed { get; private set; }

	public int numberOfDestroyedStructures { get; private set; }

	public override IPartyQuestTarget target => PlayerManager.Instance.player?.playerSettlement;

	public override Type serializedData => typeof(SaveDataCounterattackPartyQuest);

	public override bool waitingToWorkingStateImmediately => true;

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public CounterattackPartyQuest()
		: base(PARTY_QUEST_TYPE.Counterattack)
	{
		numberOfDestroyedStructures = 0;
		base.minimumPartySize = 3;
		base.priority = 4;
		base.relatedBehaviour = typeof(AttackDemonicStructureBehaviour);
	}

	public CounterattackPartyQuest(SaveDataCounterattackPartyQuest data)
		: base(data)
	{
		numberOfStructureToBeDestroyed = data.numberOfStructureToBeDestroyed;
		numberOfDestroyedStructures = data.numberOfDestroyedStructures;
	}

	public override void OnAcceptQuest(Party partyThatAcceptedQuest)
	{
		base.OnAcceptQuest(partyThatAcceptedQuest);
		Messenger.AddListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		RemoveAllCombatToDemonicStructure();
		Messenger.RemoveListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
	}

	public override void OnWaitTimeOver()
	{
		base.OnWaitTimeOver();
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			character.traitContainer.AddTrait(character, "Fervor");
		}
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return PlayerManager.Instance.player.playerSettlement;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName;
	}

	public override void OnRemoveMemberThatJoinedQuest(Character character)
	{
		base.OnRemoveMemberThatJoinedQuest(character);
		character.traitContainer.RemoveTrait(character, "Fervor");
	}

	public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState)
	{
		base.OnAssignedPartySwitchedState(fromState, toState);
		if (toState == PARTY_STATE.Working)
		{
			bool hasEndQuest = false;
			CultistBetrayalProcessing(ref hasEndQuest);
		}
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			return PlayerManager.Instance.player.playerSettlement != null;
		}
		return false;
	}

	private void RemoveAllCombatToDemonicStructure()
	{
		if (base.assignedParty == null)
		{
			return;
		}
		for (int i = 0; i < base.assignedParty.membersThatJoinedQuest.Count; i++)
		{
			Character character = base.assignedParty.membersThatJoinedQuest[i];
			if (!character.combatComponent.isInCombat)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < character.combatComponent.hostilesInRange.Count; j++)
			{
				IPointOfInterest pointOfInterest = character.combatComponent.hostilesInRange[j];
				if (pointOfInterest is TileObject { isDamageContributorToStructure: not false } tileObject && tileObject.structureLocation.structureType.IsPlayerStructure() && character.combatComponent.RemoveHostileInRange(pointOfInterest, processCombatBehavior: false))
				{
					flag = true;
					j--;
				}
			}
			if (flag)
			{
				character.combatComponent.SetWillProcessCombat(state: true);
			}
		}
	}

	public void SetNumberOfStructuresToBeDestroyed(int p_amount)
	{
		numberOfStructureToBeDestroyed = p_amount;
	}

	private void OnStructureDestroyedBy(LocationStructure p_structure, Character p_responsibleCharacter)
	{
		if (base.assignedParty != null && p_structure.structureType.IsPlayerStructure() && p_responsibleCharacter.partyComponent.IsAMemberOfParty(base.assignedParty) && p_responsibleCharacter.partyComponent.isMemberThatJoinedQuest)
		{
			numberOfDestroyedStructures++;
			if (numberOfDestroyedStructures >= numberOfStructureToBeDestroyed)
			{
				EndQuest(PartyQuest.GetLocalizedEndQuestReason("Finished_Quest"));
			}
		}
	}

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
	}

	public override void LoadReferencesInMainThread(SaveDataPartyQuest data)
	{
		base.LoadReferencesInMainThread(data);
		Messenger.AddListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
	}
}
