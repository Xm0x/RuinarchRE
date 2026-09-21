public class MinkShirt : ArmorItem
{
	public MinkShirt()
	{
		Initialize(TILE_OBJECT_TYPE.MINK_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MinkShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
