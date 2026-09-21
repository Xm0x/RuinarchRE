public class OrichalcumArmor : ArmorItem
{
	public OrichalcumArmor()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
