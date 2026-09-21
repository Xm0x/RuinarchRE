public class OrichalcumDagger : WeaponItem
{
	public OrichalcumDagger()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_DAGGER, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumDagger(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
