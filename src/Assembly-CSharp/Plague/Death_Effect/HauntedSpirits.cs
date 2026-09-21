using Inner_Maps;
using UtilityScripts;

namespace Plague.Death_Effect;

public class HauntedSpirits : PlagueDeathEffect
{
	private TILE_OBJECT_TYPE[] _spirits = new TILE_OBJECT_TYPE[3]
	{
		TILE_OBJECT_TYPE.RAVENOUS_SPIRIT,
		TILE_OBJECT_TYPE.FEEBLE_SPIRIT,
		TILE_OBJECT_TYPE.FORLORN_SPIRIT
	};

	public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Haunted_Spirits;

	protected override void ActivateEffect(Character p_character)
	{
		CreateSpirits(_level, p_character);
	}

	protected override int GetNextLevelUpgradeCost()
	{
		return _level switch
		{
			1 => 50, 
			2 => 75, 
			_ => -1, 
		};
	}

	public override string GetCurrentEffectDescription()
	{
		return _level switch
		{
			1 => "1 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Random Spirit"), 
			2 => "2 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Random Spirits"), 
			3 => "3 " + LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Random Spirits"), 
			_ => string.Empty, 
		};
	}

	public override void OnDeath(Character p_character)
	{
		ActivateEffectOn(p_character);
	}

	private void CreateSpirits(int amount, Character p_character)
	{
		LocationGridTile gridTileLocation = p_character.gridTileLocation;
		if (gridTileLocation != null)
		{
			for (int i = 0; i < amount; i++)
			{
				TILE_OBJECT_TYPE tileObjectType = _spirits[GameUtilities.RandomBetweenTwoNumbers(0, _spirits.Length - 1)];
				TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
				tileObject.SetGridTileLocation(gridTileLocation);
				tileObject.OnPlacePOI();
			}
		}
	}
}
