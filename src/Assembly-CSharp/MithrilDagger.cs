public class MithrilDagger : WeaponItem
{
	public MithrilDagger()
	{
		Initialize(TILE_OBJECT_TYPE.MITHRIL_DAGGER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public MithrilDagger(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
