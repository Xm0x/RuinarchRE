using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements.Settlement_Types;

public class SaveDataSettlementType : SaveData<SettlementType>
{
	public Dictionary<StructureSetting, int> facilityWeights;

	public Dictionary<StructureSetting, int> facilityCaps;

	public SETTLEMENT_TYPE settlementType;

	public override void Save(SettlementType data)
	{
		base.Save(data);
		facilityWeights = data.facilityWeights.dictionary;
		facilityCaps = data.facilityCaps;
		settlementType = data.settlementType;
	}

	public override SettlementType Load()
	{
		return LandmarkManager.Instance.CreateSettlementType(this);
	}
}
