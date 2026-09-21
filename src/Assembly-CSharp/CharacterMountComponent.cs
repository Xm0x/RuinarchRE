using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterMountComponent : CharacterComponent
{
	public string mountedCharacterPersistentID { get; private set; }

	public BuffStatsBonus mountStatsBonus { get; private set; }

	public Character mountedCharacter => CharacterManager.Instance.GetCharacterByPersistentID(mountedCharacterPersistentID);

	public CharacterMountComponent()
	{
		mountStatsBonus = new BuffStatsBonus();
	}

	public CharacterMountComponent(SaveDataCharacterMountComponent data)
	{
		mountStatsBonus = data.mountStatsBonus;
		mountedCharacterPersistentID = data.mountedCharacter;
	}

	public bool MountCharacter(Character p_character)
	{
		if (mountedCharacterPersistentID == null && p_character != null)
		{
			mountedCharacterPersistentID = p_character.persistentID;
			if (p_character.gridTileLocation != null)
			{
				CharacterManager.Instance.Teleport(base.owner, p_character.gridTileLocation);
			}
			CharacterManager.Instance.PutCharacterIntoLimbo(p_character);
			if (base.owner.visuals != null)
			{
				base.owner.visuals.UpdateAllVisuals(base.owner);
			}
			base.owner.movementComponent.UpdateMovement();
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Mount_Character", LOG_TAG.Life_Changes);
			log.AddToFillers(base.owner, base.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(base.owner, log, releaseLogAfter: true);
			return true;
		}
		return false;
	}

	public bool Dismount()
	{
		if (base.owner.gridTileLocation != null)
		{
			Character character = mountedCharacter;
			if (character != null)
			{
				mountedCharacterPersistentID = string.Empty;
				CharacterManager.Instance.ReleaseCharacterFromLimbo(character, base.owner.gridTileLocation);
				if (base.owner.visuals != null)
				{
					base.owner.visuals.UpdateAllVisuals(base.owner);
				}
				base.owner.movementComponent.UpdateMovement();
				return true;
			}
		}
		return false;
	}

	public bool IsMounting()
	{
		return !string.IsNullOrEmpty(mountedCharacterPersistentID);
	}

	public bool TryToMountWyvernPet()
	{
		if (ChanceData.RollChance(CHANCE_TYPE.Mount_Wyvern) && CanMountWyvernPet())
		{
			return MountWyvernPet();
		}
		return false;
	}

	private bool MountWyvernPet()
	{
		Wyvern wyvernPet = base.owner.petComponent.wyvernPet;
		return MountCharacter(wyvernPet);
	}

	private bool CanMountWyvernPet()
	{
		if (base.owner.petComponent.HasWyvernPet())
		{
			Wyvern wyvernPet = base.owner.petComponent.wyvernPet;
			if (wyvernPet.limiterComponent.canPerform && wyvernPet.limiterComponent.canMove && !wyvernPet.mountComponent.IsBeingMounted())
			{
				LocationGridTile gridTileLocation = wyvernPet.gridTileLocation;
				LocationGridTile gridTileLocation2 = base.owner.gridTileLocation;
				if (base.owner.hasMarker && wyvernPet.hasMarker && gridTileLocation != null && gridTileLocation2 != null && gridTileLocation.GetDistanceTo(gridTileLocation2) <= 4f && base.owner.marker.IsCharacterInLineOfSightWith(wyvernPet) && !base.owner.isInVampireBatForm && !base.owner.isInWerewolfForm)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsBeingMounted()
	{
		return base.owner.petComponent.petOwner?.mountComponent.mountedCharacterPersistentID == base.owner.persistentID;
	}

	public Character GetRider()
	{
		if (IsBeingMounted())
		{
			return base.owner.petComponent.petOwner;
		}
		return null;
	}

	private void ApplyCombatStats()
	{
		Character character = mountedCharacter;
		mountStatsBonus.Reset();
		int num = 0;
		if (character != null)
		{
			num = ((mountedCharacter.characterClass.attackType != ATTACK_TYPE.MAGICAL) ? mountedCharacter.combatComponent.GetComputedStrength() : mountedCharacter.combatComponent.GetComputedIntelligence());
		}
		if (base.owner.characterClass.attackType == ATTACK_TYPE.MAGICAL)
		{
			mountStatsBonus.SetIntelligence(num);
		}
		else
		{
			mountStatsBonus.SetStrength(num);
		}
		base.owner.combatComponent.AdjustStrengthModifier(mountStatsBonus.strength);
		base.owner.combatComponent.AdjustIntelligenceModifier(mountStatsBonus.intelligence);
	}

	private void UnapplyCombatStats()
	{
		base.owner.combatComponent.AdjustStrengthModifier(-mountStatsBonus.strength);
		base.owner.combatComponent.AdjustIntelligenceModifier(-mountStatsBonus.intelligence);
	}

	public void TryAttack(IDamageable p_target)
	{
		SpecialSkill(p_target);
	}

	private void SpecialSkill(IDamageable p_target)
	{
		Character character = mountedCharacter;
		if (character != null && character.combatComponent.specialSkillParent.HasSpecialSkill())
		{
			character.combatComponent.specialSkillParent.TryActivateSpecialSkill(character);
		}
	}

	public void LoadReferences(SaveDataCharacterMountComponent data)
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = mountedCharacter;
	}
}
