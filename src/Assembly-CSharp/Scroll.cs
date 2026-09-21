public class Scroll : AccessoryItem
{
	public Scroll()
	{
		Initialize(TILE_OBJECT_TYPE.SCROLL, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public Scroll(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
