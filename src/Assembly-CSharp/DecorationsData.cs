public class DecorationsData : BuildPlayerSkill
{
	private TILE_OBJECT_TYPE[] _decorationsType = new TILE_OBJECT_TYPE[27]
	{
		TILE_OBJECT_TYPE.BLOOD_POOL,
		TILE_OBJECT_TYPE.BONES,
		TILE_OBJECT_TYPE.CAGE,
		TILE_OBJECT_TYPE.CANDLES,
		TILE_OBJECT_TYPE.CAULDRON,
		TILE_OBJECT_TYPE.CHAINS,
		TILE_OBJECT_TYPE.CORRUPTED_PIT,
		TILE_OBJECT_TYPE.CORRUPTED_SPIKE,
		TILE_OBJECT_TYPE.CORRUPTED_TENDRIL,
		TILE_OBJECT_TYPE.CRYPT_CHEST,
		TILE_OBJECT_TYPE.DEMON_ALTAR,
		TILE_OBJECT_TYPE.DEMON_CIRCLE,
		TILE_OBJECT_TYPE.DEMON_RACK,
		TILE_OBJECT_TYPE.DEMON_THRONE,
		TILE_OBJECT_TYPE.FEEDING_TROUGH,
		TILE_OBJECT_TYPE.GRIMOIRE,
		TILE_OBJECT_TYPE.JARS,
		TILE_OBJECT_TYPE.MANA_RUNE,
		TILE_OBJECT_TYPE.MANACLES,
		TILE_OBJECT_TYPE.PEW,
		TILE_OBJECT_TYPE.SIGIL,
		TILE_OBJECT_TYPE.SKULL_LAMP,
		TILE_OBJECT_TYPE.SKULLS,
		TILE_OBJECT_TYPE.SPAWNING_PIT,
		TILE_OBJECT_TYPE.TEMPLE_ALTAR,
		TILE_OBJECT_TYPE.TORTURE_TABLE,
		TILE_OBJECT_TYPE.SMALL_TREE_OBJECT
	};

	private TILE_OBJECT_TYPE _chosenTileObjectType;

	public override string name => "Decorations";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DECORATIONS;

	public TILE_OBJECT_TYPE chosenTileObjectType => _chosenTileObjectType;

	public TILE_OBJECT_TYPE[] decorationsType => _decorationsType;

	public DecorationsData()
	{
		_chosenTileObjectType = TILE_OBJECT_TYPE.NONE;
	}

	public override void OnSetAsCurrentActiveSpell()
	{
		base.OnSetAsCurrentActiveSpell();
		PlayerUI.Instance.decorationsUI.Open();
	}

	public override void OnNoLongerCurrentActiveSpell()
	{
		base.OnNoLongerCurrentActiveSpell();
		PlayerUI.Instance.decorationsUI.Close();
	}

	public void SetChosenTileObjectTypeIndex(int p_index)
	{
		BaseBuildingManager.Instance.SetHasClickedLeft(p_state: false);
		BaseBuildingManager.Instance.ResetOnMouseRelease();
		if (p_index >= 0)
		{
			_chosenTileObjectType = decorationsType[p_index];
		}
		else
		{
			_chosenTileObjectType = TILE_OBJECT_TYPE.NONE;
		}
	}

	public override void ActivateAbility(int p_numberOfTimesToBeExecuted)
	{
		base.ActivateAbility(p_numberOfTimesToBeExecuted);
		if ((base.hasCharges && base.charges <= 0 && base.bonusCharges <= 0) || (base.hasSpiritEnergyCost && PlayerManager.Instance.player.currenciesComponent.spiritEnergy < base.spiritEnergyCost))
		{
			Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
		}
	}
}
