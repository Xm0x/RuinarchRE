public class Bracer : AccessoryItem
{
	public Bracer()
	{
		Initialize(TILE_OBJECT_TYPE.BRACER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public Bracer(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
