public class MithrilAxe : WeaponItem
{
	public MithrilAxe()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_AXE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilAxe(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
