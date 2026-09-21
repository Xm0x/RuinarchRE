public class Belt : AccessoryItem
{
	public Belt()
	{
		Initialize(TILE_OBJECT_TYPE.BELT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public Belt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
