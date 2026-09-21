public class ScaleArmor : ArmorItem
{
	public ScaleArmor()
	{
		Initialize(TILE_OBJECT_TYPE.SCALE_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public ScaleArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
