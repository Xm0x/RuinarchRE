public class OrichalcumAxe : WeaponItem
{
	public OrichalcumAxe()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_AXE, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumAxe(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
