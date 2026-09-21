public class Ring : AccessoryItem
{
	public Ring()
	{
		Initialize(TILE_OBJECT_TYPE.RING, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public Ring(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
