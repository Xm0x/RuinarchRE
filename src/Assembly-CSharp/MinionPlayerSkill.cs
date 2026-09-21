using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MinionPlayerSkill : SkillData
{
	public MINION_TYPE minionType = MINION_TYPE.Envy;

	private string _cannotSummonOnUncorruptedTilesInvalidText;

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.MINION;

	public RACE race { get; protected set; }

	public string className { get; protected set; }

	public MinionPlayerSkill()
	{
		race = RACE.DEMON;
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_cannotSummonOnUncorruptedTilesInvalidText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Summon_Not_Corrupted_Invalid");
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		Minion minion = CharacterManager.Instance.CreateNewMinion(className, RACE.DEMON, initialize: false);
		minion.SetMinionPlayerSkillType(type);
		minion.SetMinionType(minionType);
		minion.Summon(targetTile);
		if (targetTile.IsPartOfSettlement(out var settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE && targetTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN)
		{
			minion.character.MigrateHomeStructureTo(targetTile.structure);
		}
		else
		{
			minion.character.SetTerritory(targetTile.area, returnHome: false);
		}
		minion.character.jobQueue.CancelAllJobs();
		PlayerManager.Instance.player.underlingsComponent.AddCharacterToPersistentDefendParty(minion.character);
		Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_DEFENDER, minion.character);
		if (!PlayerSkillManager.Instance.unlimitedCast)
		{
			if (base.hasBonusCharges)
			{
				AdjustBonusCharges(-1);
			}
			else if (base.hasCharges && base.charges > 0 && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
			{
				AdjustCharges(-1);
			}
		}
	}

	public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter)
	{
		Minion minion = CharacterManager.Instance.CreateNewMinion(className, RACE.DEMON, initialize: false);
		minion.SetMinionPlayerSkillType(type);
		minion.SetMinionType(minionType);
		minion.Summon(targetTile);
		spawnedCharacter = minion.character;
		if (!PlayerSkillManager.Instance.unlimitedCast)
		{
			if (base.hasBonusCharges)
			{
				AdjustBonusCharges(-1);
			}
			else if (base.hasCharges && base.charges > 0 && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(type))
			{
				AdjustCharges(-1);
			}
		}
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		o_cannotPerformReason = string.Empty;
		if (targetTile.structure is Kennel)
		{
			return false;
		}
		if (targetTile.structure.IsTilePartOfARoom(targetTile, out var room) && room is PrisonCell)
		{
			return false;
		}
		if (!targetTile.IsPassable())
		{
			return false;
		}
		if (!targetTile.corruptionComponent.isCorrupted)
		{
			o_cannotPerformReason = _cannotSummonOnUncorruptedTilesInvalidText;
			return false;
		}
		return true;
	}

	public override void FinishCooldown()
	{
		base.FinishCooldown();
		SetCooldown(-1);
	}

	protected override void PerTickCooldown()
	{
		base.PerTickCooldown();
		if (base.currentCooldownTick < base.cooldown)
		{
			Messenger.Broadcast(PlayerSkillSignals.PER_TICK_DEMON_COOLDOWN, this);
		}
	}
}
