using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DemonRescuePartyQuest : PartyQuest, IRescuePartyQuest
{
	public Character targetCharacter { get; private set; }

	public DemonicStructure targetDemonicStructure { get; private set; }

	public List<LocationGridTile> targetDemonicStructureTiles { get; private set; }

	public bool isReleasing { get; private set; }

	public override IPartyQuestTarget target => targetCharacter;

	public override Type serializedData => typeof(SaveDataDemonRescuePartyQuest);

	public override bool waitingToWorkingStateImmediately => true;

	public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;

	public DemonRescuePartyQuest()
		: base(PARTY_QUEST_TYPE.Demon_Rescue)
	{
		base.minimumPartySize = 1;
		base.priority = 5;
		base.relatedBehaviour = typeof(DemonRescueBehaviour);
		targetDemonicStructureTiles = new List<LocationGridTile>();
	}

	public DemonRescuePartyQuest(SaveDataDemonRescuePartyQuest data)
		: base(data)
	{
		targetDemonicStructureTiles = new List<LocationGridTile>();
		isReleasing = data.isReleasing;
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		if (!targetDemonicStructure.hasBeenDestroyed)
		{
			return targetDemonicStructure;
		}
		if (targetCharacter.currentStructure != null && targetCharacter.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS)
		{
			return targetCharacter.currentStructure;
		}
		if (targetCharacter.gridTileLocation != null)
		{
			return targetCharacter.areaLocation;
		}
		return base.GetTargetDestination();
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName + " " + targetCharacter.name;
	}

	protected override void OnEndQuest()
	{
		base.OnEndQuest();
		RemoveAllCombatToDemonicStructure();
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (targetDemonicStructure != null && targetCharacter != null)
		{
			return targetCharacter.traitContainer.HasTrait("Restrained");
		}
		return false;
	}

	public override bool IsInterestedInJoiningQuest(Character p_character)
	{
		if (p_character == targetCharacter)
		{
			return false;
		}
		if (p_character.relationshipContainer.HasGrudgeAgainst(targetCharacter))
		{
			return false;
		}
		if (targetCharacter != null)
		{
			return !p_character.relationshipContainer.IsEnemiesWith(targetCharacter);
		}
		return base.IsInterestedInJoiningQuest(p_character);
	}

	protected override bool IsConnectedToStructure(LocationStructure p_structure)
	{
		if (targetDemonicStructure == p_structure)
		{
			return true;
		}
		return base.IsConnectedToStructure(p_structure);
	}

	public void SetTargetCharacter(Character character)
	{
		targetCharacter = character;
	}

	public void SetTargetDemonicStructure(DemonicStructure p_targetStructure)
	{
		targetDemonicStructure = p_targetStructure;
		UpdateDemonicStructureTiles();
	}

	public void SetIsReleasing(bool state)
	{
		if (isReleasing != state)
		{
			isReleasing = state;
			if (isReleasing)
			{
				RemoveAllCombatToDemonicStructure();
			}
		}
	}

	private void UpdateDemonicStructureTiles()
	{
		targetDemonicStructureTiles.Clear();
		if (targetDemonicStructure != null)
		{
			for (int i = 0; i < targetDemonicStructure.tiles.Count; i++)
			{
				targetDemonicStructureTiles.Add(targetDemonicStructure.tiles.ElementAt(i));
			}
		}
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
				if (pointOfInterest is TileObject { isDamageContributorToStructure: not false } tileObject && tileObject.structureLocation == targetDemonicStructure && character.combatComponent.RemoveHostileInRange(pointOfInterest, processCombatBehavior: false))
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

	public override void LoadReferences(SaveDataPartyQuest data)
	{
		base.LoadReferences(data);
		if (data is SaveDataDemonRescuePartyQuest saveDataDemonRescuePartyQuest)
		{
			if (!string.IsNullOrEmpty(saveDataDemonRescuePartyQuest.targetCharacter))
			{
				targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataDemonRescuePartyQuest.targetCharacter);
			}
			if (!string.IsNullOrEmpty(saveDataDemonRescuePartyQuest.targetDemonicStructure))
			{
				targetDemonicStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataDemonRescuePartyQuest.targetDemonicStructure) as DemonicStructure;
				UpdateDemonicStructureTiles();
			}
		}
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = targetDemonicStructure;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
	}
}
