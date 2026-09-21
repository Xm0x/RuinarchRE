public class WoolShirt : ArmorItem
{
	public WoolShirt()
	{
		Initialize(TILE_OBJECT_TYPE.WOOL_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public WoolShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
