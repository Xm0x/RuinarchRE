public class RabbitShirt : ArmorItem
{
	public RabbitShirt()
	{
		Initialize(TILE_OBJECT_TYPE.RABBIT_SHIRT, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		EquipmentBonusProcessor.SetBonusResistanceOnWeapon(this);
	}

	public RabbitShirt(SaveDataEquipmentItem data)
		: base(data)
	{
	}
}
