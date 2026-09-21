public class Necklace : AccessoryItem
{
	public Necklace()
	{
		Initialize(TILE_OBJECT_TYPE.NECKLACE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public Necklace(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
