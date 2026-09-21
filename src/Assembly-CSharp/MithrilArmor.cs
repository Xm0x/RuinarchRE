public class MithrilArmor : ArmorItem
{
	public MithrilArmor()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
