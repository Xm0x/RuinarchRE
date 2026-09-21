public class MithrilSword : WeaponItem
{
	public MithrilSword()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_SWORD, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilSword(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
