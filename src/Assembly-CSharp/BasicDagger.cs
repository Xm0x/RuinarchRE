public class BasicDagger : WeaponItem
{
	public BasicDagger()
	{
		Initialize(TILE_OBJECT_TYPE.BASIC_DAGGER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BasicDagger(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
