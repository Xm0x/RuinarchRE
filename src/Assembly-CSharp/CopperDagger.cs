public class CopperDagger : WeaponItem
{
	public CopperDagger()
	{
		Initialize(TILE_OBJECT_TYPE.COPPER_DAGGER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public CopperDagger(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
