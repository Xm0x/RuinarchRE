using UnityEngine;

public class BreedMonsterData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BREED_MONSTER;

	public override string name => "Breed Monster";

	public override string description => "This Action adds 1 Charge of the current monster to the player's Monsters List.";

	public BreedMonsterData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Summon summon)
		{
			ObjectPoolManager.Instance.InstantiateObjectFromPool("Breed Effect", summon.worldPosition, Quaternion.identity, summon.currentRegion.innerMap.objectsParent, isWorldPosition: true).GetComponent<BreedEffect>().PlayEffect(summon.marker.usedSprite);
			base.ActivateAbility(targetPOI);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter is Summon && !targetCharacter.isDead && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null)
			{
				return targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL;
			}
			return false;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Summon summon)
		{
			if (base.IsValid(target) && summon.gridTileLocation != null && summon.gridTileLocation.structure != null && summon.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL && !(summon is Dragon))
			{
				return PlayerSkillManager.Instance.GetSummonPlayerSkillData(summon.race, summon.characterClass.className) != null;
			}
			return false;
		}
		return false;
	}
}
