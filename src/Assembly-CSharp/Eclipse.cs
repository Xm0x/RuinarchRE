using Inner_Maps.Location_Structures;
using UtilityScripts;

public class Eclipse : WeaponItem
{
	public Eclipse()
	{
		Initialize(TILE_OBJECT_TYPE.ECLIPSE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Eclipse(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public override void ApplyWeaponEffectsOnHit(IPointOfInterest p_targetPOI)
	{
		base.ApplyWeaponEffectsOnHit(p_targetPOI);
		bool flag = false;
		if (p_targetPOI is TileObject tileObject && tileObject.gridTileLocation?.structure is DemonicStructure demonicStructure && demonicStructure.objectsThatContributeToDamage.Contains(tileObject))
		{
			flag = true;
		}
		else if (p_targetPOI is Character { faction: not null } character && character.faction.factionType.type == FACTION_TYPE.Demons)
		{
			flag = true;
		}
		if (flag && PlayerManager.Instance.player.currenciesComponent.mana > 0)
		{
			int num = GameUtilities.RandomBetweenTwoNumbers(10, 50);
			PlayerManager.Instance.player.currenciesComponent.AdjustMana(-num);
		}
	}
}
