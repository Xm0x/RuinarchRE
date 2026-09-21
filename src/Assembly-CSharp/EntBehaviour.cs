using Inner_Maps;
using Inner_Maps.Location_Structures;

public class EntBehaviour : BaseMonsterBehaviour
{
	private readonly WeightedDictionary<string> _actionWeights;

	public EntBehaviour()
	{
		base.priority = 9;
		_actionWeights = new WeightedDictionary<string>();
		_actionWeights.AddElement("Roam", 50);
		_actionWeights.AddElement("Stand", 20);
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.gridTileLocation != null && !InnerMapManager.Instance.CanBigTreeBePlacedOnTile(character.gridTileLocation))
		{
			_actionWeights.SetElementWeight("Revert", 0);
		}
		else
		{
			_actionWeights.SetElementWeight("Revert", 30);
		}
		string text = _actionWeights.PickRandomElementGivenWeights();
		if (text == "Revert" && character.currentStructure is Kennel)
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
		Ent ent = character as Ent;
		ent.SetIsTree(state: true);
		LocationGridTile gridTileLocation = character.gridTileLocation;
		character.marker.SetVisualState(state: false);
		character.marker.SetLightState(p_state: false);
		TreeObject treeObject = InnerMapManager.Instance.CreateNewTileObject<TreeObject>(TILE_OBJECT_TYPE.BIG_TREE_OBJECT);
		gridTileLocation.structure.AddPOI(treeObject, gridTileLocation);
		treeObject.SetOccupyingEnt(ent);
		return true;
	}
}
