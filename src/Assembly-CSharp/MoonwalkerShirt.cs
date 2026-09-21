public class MoonwalkerShirt : ArmorItem
{
	public MoonwalkerShirt()
	{
		Initialize(TILE_OBJECT_TYPE.MOONWALKER_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MoonwalkerShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
