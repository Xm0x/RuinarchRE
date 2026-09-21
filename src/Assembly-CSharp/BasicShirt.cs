public class BasicShirt : ArmorItem
{
	public BasicShirt()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
