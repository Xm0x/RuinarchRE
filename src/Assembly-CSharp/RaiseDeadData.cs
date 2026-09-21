using Object_Pools;

public class RaiseDeadData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.RAISE_DEAD;

	public override string name => "Raise Dead";

	public override string description => "This Action can be used on a Villager corpse to spawn a Skeleton. The Skeleton will belong to the Undead Faction.";

	public RaiseDeadData()
	{
		base.targetTypes = new SPELL_TARGET[2]
		{
			SPELL_TARGET.CHARACTER,
			SPELL_TARGET.TILE_OBJECT
		};
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character character = null;
		if (targetPOI is Character character2)
		{
			character = character2;
		}
		else if (targetPOI is Tombstone tombstone)
		{
			character = tombstone.character;
		}
		if (character.grave != null && character.grave.isBeingCarriedBy != null)
		{
			character.grave.isBeingCarriedBy.UncarryPOI(character.grave);
		}
		Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)character, "");
		Summon summon = CharacterManager.Instance.RaiseFromDeadReplaceCharacterWithMonsterType(SUMMON_TYPE.Skeleton, character, FactionManager.Instance.undeadFaction);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "player_raise_dead", LOG_TAG.Player, LOG_TAG.Life_Changes);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
		LogPool.Release(log);
		if (UIManager.Instance.characterInfoUI.isShowing)
		{
			UIManager.Instance.characterInfoUI.CloseMenu();
		}
		base.ActivateAbility(targetPOI);
		summon.combatComponent.AdjustMaxHPPercentModifier(PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.RAISE_DEAD));
		summon.combatComponent.AdjustAttackPercentModifier(PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.RAISE_DEAD));
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (!targetCharacter.isDead || !targetCharacter.carryComponent.IsNotBeingCarried() || targetCharacter.marker == null || targetCharacter.characterClass.IsZombie())
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (targetCharacter.characterClass.IsZombie())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Raise_Dead_Zombie") + "|";
		}
		return text;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		if (targetTileObject is Tombstone { character: not null } tombstone && tombstone.character.traitContainer.IsBlessed())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Cannot_Target_Blessed") + "|";
		}
		return text;
	}

	public override bool CanPerformAbilityTowards(TileObject tileObject)
	{
		if (tileObject is Tombstone tombstone)
		{
			return CanPerformAbilityTowards(tombstone.character);
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		Character character = null;
		if (target is Tombstone tombstone)
		{
			character = tombstone.character;
		}
		else if (target is Character character2)
		{
			character = character2;
		}
		if (character != null)
		{
			if (!character.isDead)
			{
				return false;
			}
			if (!character.race.IsSapient())
			{
				return false;
			}
		}
		bool flag = base.IsValid(target);
		if (!flag && target is Tombstone { isBeingCarriedBy: not null })
		{
			return true;
		}
		return flag;
	}
}
