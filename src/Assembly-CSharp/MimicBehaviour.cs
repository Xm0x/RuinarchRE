using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MimicBehaviour : BaseMonsterBehaviour
{
	private readonly WeightedDictionary<string> _actionWeights;

	public MimicBehaviour()
	{
		base.priority = 9;
		_actionWeights = new WeightedDictionary<string>();
		_actionWeights.AddElement("Roam", 50);
		_actionWeights.AddElement("Stand", 20);
		_actionWeights.AddElement("Revert", 5);
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		string text = _actionWeights.PickRandomElementGivenWeights();
		if (character.currentStructure is Kennel && text == "Revert")
		{
			text = "Stand";
		}
		if (text == "Roam")
		{
			return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
		}
		if (text == "Stand")
		{
			return character.jobComponent.TriggerStandStill(out producedJob);
		}
		producedJob = null;
		Mimic mimic = character as Mimic;
		mimic.SetIsTreasureChest(state: true);
		LocationGridTile gridTileLocation = character.gridTileLocation;
		character.marker.SetVisualState(state: false);
		character.marker.SetLightState(p_state: false);
		TreasureChest treasureChest = InnerMapManager.Instance.CreateNewTileObject<TreasureChest>(TILE_OBJECT_TYPE.TREASURE_CHEST);
		gridTileLocation.structure.AddPOI(treasureChest, gridTileLocation);
		treasureChest.SetObjectInside(mimic);
		return true;
	}
}
