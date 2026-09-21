public class OrichalcumSword : WeaponItem
{
	public OrichalcumSword()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_SWORD, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumSword(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
