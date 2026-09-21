public class BoarHideArmor : ArmorItem
{
	public BoarHideArmor()
	{
		Initialize(TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public BoarHideArmor(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
