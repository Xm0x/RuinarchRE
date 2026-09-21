public class WolfHideArmor : ArmorItem
{
	public WolfHideArmor()
	{
		Initialize(TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public WolfHideArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
