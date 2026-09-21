using System;
using Inner_Maps;
using UnityEngine;

public class DemonDefendPartyQuest : PartyQuest
{
	public override IPartyQuestTarget target => PlayerManager.Instance?.player?.playerSettlement;

	public override Type serializedData => typeof(SaveDataDemonDefendPartyQuest);

	public override bool workingStateImmediately => true;

	public DemonDefendPartyQuest()
		: base(PARTY_QUEST_TYPE.Demon_Defend)
	{
		base.minimumPartySize = 0;
		base.priority = 5;
		base.relatedBehaviour = typeof(DemonDefendBehaviour);
	}

	public DemonDefendPartyQuest(SaveDataDemonDefendPartyQuest data)
		: base(data)
	{
	}

	public override IPartyTargetDestination GetTargetDestination()
	{
		return PlayerManager.Instance?.player?.playerSettlement;
	}

	public override string GetPartyQuestName()
	{
		return base.localizedPartialQuestName;
	}

	public override bool IsStillEligibleFor(Faction p_faction)
	{
		if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
		{
			return PlayerManager.Instance.player.playerSettlement != null;
		}
		return false;
	}

	public bool IsDefenderNearEnoughToTarget(Character p_defender, IPointOfInterest p_target)
	{
		if (p_defender.hasMarker && p_target.mapObjectVisual != null)
		{
			return Vector2.Distance(p_defender.worldPosition, p_target.worldPosition) <= 15f;
		}
		return false;
	}

	public bool IsPOIInPlayerSettlementOrCorruptedTileAndNotInPrisonOrKennel(IPointOfInterest p_poi)
	{
		LocationGridTile gridTileLocation = p_poi.gridTileLocation;
		if (gridTileLocation != null)
		{
			gridTileLocation.IsPartOfSettlement(out var settlement);
			if ((settlement == PlayerManager.Instance.player.playerSettlement || gridTileLocation.corruptionComponent.isCorrupted) && gridTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL)
			{
				return gridTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS;
			}
			return false;
		}
		return false;
	}

	public void OnDefenderStartedAttackingAnIntruder(Character p_character, IPointOfInterest p_target)
	{
		if (!IsPOIInPlayerSettlementOrCorruptedTileAndNotInPrisonOrKennel(p_character))
		{
			return;
		}
		CombatData combatData = p_character.combatComponent.GetCombatData(p_target);
		if (combatData == null)
		{
			return;
		}
		for (int i = 0; i < base.assignedParty.members.Count; i++)
		{
			Character character = base.assignedParty.members[i];
			if (character != p_character && !character.combatComponent.isInCombat && IsDefenderNearEnoughToTarget(character, p_target) && (!character.movementComponent.isStationary || character.marker.IsCharacterInLineOfSightWith(p_target)))
			{
				character.combatComponent.Fight(p_target, combatData.reasonForCombat, combatData.connectedAction, combatData.isLethal);
			}
		}
	}
}
