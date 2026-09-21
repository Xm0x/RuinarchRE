public class OrichalcumBow : WeaponItem
{
	public OrichalcumBow()
	{
		Initialize(TILE_OBJECT_TYPE.ORICHALCUM_BOW, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public OrichalcumBow(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
