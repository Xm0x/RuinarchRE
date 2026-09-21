public class SpiderSilkShirt : ArmorItem
{
	public SpiderSilkShirt()
	{
		Initialize(TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public SpiderSilkShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
