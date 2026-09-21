using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SummonPlayerSkill : SkillData
{
	private string _cannotSummonOnUncorruptedTilesInvalidText;

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SUMMON;

	public RACE race { get; protected set; }

	public string className { get; protected set; }

	public SUMMON_TYPE summonType { get; protected set; }

	public SummonPlayerSkill()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
		_cannotSummonOnUncorruptedTilesInvalidText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Summon_Not_Corrupted_Invalid");
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, null, targetTile.parentMap.region, null, className);
		summon.SetDestroyMarkerOnDeath(state: true);
		summon.OnSummonAsPlayerMonster();
		CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
		if (targetTile.structure.structureType != STRUCTURE_TYPE.WILDERNESS && targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN && targetTile.IsPartOfSettlement(out var settlement) && settlement.locationType != LOCATION_TYPE.VILLAGE)
		{
			summon.MigrateHomeStructureTo(targetTile.structure);
		}
		else
		{
			summon.SetTerritory(targetTile.area, returnHome: false);
		}
		summon.jobQueue.CancelAllJobs();
		PlayerManager.Instance.player.underlingsComponent.AddCharacterToPersistentDefendParty(summon);
		Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_DEFENDER, (Character)summon);
	}

	public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter)
	{
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, null, targetTile.parentMap.region, null, className);
		summon.SetDestroyMarkerOnDeath(state: true);
		summon.OnSummonAsPlayerMonster();
		CharacterManager.Instance.PlaceSummonInitially(summon, targetTile);
		spawnedCharacter = summon;
		PlayerManager.Instance.player.underlingsComponent.AddCharacterToPersistentDefendParty(summon);
		Messenger.Broadcast(PlayerSignals.PLAYER_PLACED_DEFENDER, (Character)summon);
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

	public override void OnNoLongerCurrentActiveSpell()
	{
		base.OnNoLongerCurrentActiveSpell();
	}

	protected virtual void AfterSummoning(Summon summon)
	{
	}
}
