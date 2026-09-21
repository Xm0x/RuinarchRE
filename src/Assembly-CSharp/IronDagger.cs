public class IronDagger : WeaponItem
{
	public IronDagger()
	{
		Initialize(TILE_OBJECT_TYPE.IRON_DAGGER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public IronDagger(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
