using System.Collections.Generic;
using Locations.Settlements;

public static class SettlementListExtension
{
	public static void PopulateSettlementsThatAreUnownedOrHostileWithFaction(this List<BaseSettlement> p_settlements, List<BaseSettlement> chosenSettlements, LOCATION_TYPE p_locationType, Faction p_otherFaction)
	{
		for (int i = 0; i < p_settlements.Count; i++)
		{
			BaseSettlement baseSettlement = p_settlements[i];
			if (baseSettlement.locationType == p_locationType && (baseSettlement.owner == null || baseSettlement.owner.IsHostileWith(p_otherFaction)))
			{
				chosenSettlements.Add(baseSettlement);
			}
		}
	}
}
