using Inner_Maps.Location_Structures;

public class Savage : WeaponItem
{
	public Savage()
	{
		Initialize(TILE_OBJECT_TYPE.SAVAGE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Savage(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public override void ApplyAttackPowerChanges(IPointOfInterest p_targetPOI, ref int attackPower)
	{
		base.ApplyAttackPowerChanges(p_targetPOI, ref attackPower);
		if (p_targetPOI is TileObject tileObject)
		{
			if (tileObject.gridTileLocation?.structure is DemonicStructure demonicStructure && demonicStructure.objectsThatContributeToDamage.Contains(tileObject))
			{
				attackPower *= 3;
			}
		}
		else if (p_targetPOI is Character character && character.raceSetting.category == CHARACTER_CATEGORY.Demonic)
		{
			attackPower *= 3;
		}
	}
}
