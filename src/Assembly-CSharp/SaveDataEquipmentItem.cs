using System.Collections.Generic;
using Inner_Maps;

public class SaveDataEquipmentItem : SaveDataTileObject
{
	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();

	public EQUIPMENT_SLAYER_BONUS randomSlayerBonus;

	public EQUIPMENT_WARD_BONUS randomWardBonus;

	public List<EQUIPMENT_BONUS> addedBonus = new List<EQUIPMENT_BONUS>();

	public EQUIPMENT_PREFIX prefix;

	public ELEMENTAL_TYPE addedElementalBonus;

	public int addedCritRate;

	public float strPercentageReduced;

	public float intPercentageReduced;

	public bool isDeadly;

	public bool isFestering;

	public bool isHaunted;

	public bool isMentor;

	public string prefixName = string.Empty;

	public GameDate expiryDate;

	public override void Save(TileObject tileObject)
	{
		base.Save(tileObject);
		EquipmentItem equipmentItem = tileObject as EquipmentItem;
		equipmentItem.resistanceBonuses.ForEach(delegate(RESISTANCE eachRes)
		{
			resistanceBonuses.Add(eachRes);
		});
		randomSlayerBonus = equipmentItem.addedSlayerBonus;
		randomWardBonus = equipmentItem.addedWardBonus;
		equipmentItem.addedBonus.ForEach(delegate(EQUIPMENT_BONUS eachBonus)
		{
			addedBonus.Add(eachBonus);
		});
		addedElementalBonus = equipmentItem.addedElementalBonus;
		addedCritRate = equipmentItem.addedCritRate;
		strPercentageReduced = equipmentItem.strPercentageReduced;
		intPercentageReduced = equipmentItem.intPercentageReduced;
		isDeadly = equipmentItem.isDeadly;
		isFestering = equipmentItem.isFestering;
		isHaunted = equipmentItem.isHaunted;
		isMentor = equipmentItem.isMentor;
		prefixName = equipmentItem.prefixName;
		prefix = equipmentItem.prefix;
		expiryDate = equipmentItem.expiryDate;
	}

	public override TileObject Load()
	{
		TileObject tileObject = InnerMapManager.Instance.LoadTileObject<TileObject>(this);
		tileObject.Initialize(this);
		return tileObject;
	}
}
