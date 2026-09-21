public class MithrilBow : WeaponItem
{
	public MithrilBow()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_BOW, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilBow(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
