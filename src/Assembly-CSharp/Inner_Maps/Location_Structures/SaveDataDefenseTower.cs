using System.Collections.Generic;

namespace Inner_Maps.Location_Structures;

public class SaveDataDefenseTower : SaveDataManMadeStructure
{
	public int remainingShots;

	public List<GameDate> remainingShotCooldowns;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		DefenseTower defenseTower = locationStructure as DefenseTower;
		remainingShots = defenseTower.remainingShots;
		remainingShotCooldowns = defenseTower.remainingShotCooldowns;
	}
}
