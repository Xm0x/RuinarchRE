using Inner_Maps.Location_Structures;
using Locations.Settlements;

public interface IPartyQuestTarget
{
	LocationStructure currentStructure { get; }

	BaseSettlement currentSettlement { get; }
}
