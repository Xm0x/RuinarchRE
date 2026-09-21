using Inner_Maps.Location_Structures;
using Locations.Settlements;

public interface IGatheringTarget
{
	LocationStructure currentStructure { get; }

	BaseSettlement currentSettlement { get; }
}
