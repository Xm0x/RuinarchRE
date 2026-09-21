using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SubterraneanBehaviour : CharacterBehaviour
{
	public SubterraneanBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (!character.isDead && character.gridTileLocation != null && !character.isNormalCharacter && !IsTamedMonster(character) && character.behaviourComponent.subterraneanJustExitedCombat)
		{
			List<LocationStructure> structuresAtLocation = character.currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.CAVE);
			List<LocationStructure> list = RuinarchListPool<LocationStructure>.Claim();
			list.AddRange(structuresAtLocation);
			list.Remove(character.currentStructure);
			LocationStructure locationStructure = null;
			if (list.Count > 0)
			{
				locationStructure = CollectionUtilities.GetRandomElement(list);
			}
			RuinarchListPool<LocationStructure>.Release(list);
			if (locationStructure != null)
			{
				LocationGridTile randomElement = CollectionUtilities.GetRandomElement(locationStructure.passableTiles);
				if (randomElement != null)
				{
					LeaveWurmHoles(character.gridTileLocation);
					CharacterManager.Instance.Teleport(character, randomElement);
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Subterranean burrow", LOG_TAG.Combat);
					log2.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log2.AddToFillers(locationStructure, locationStructure.GetNameRelativeTo(character), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
					log2.AddLogToDatabase(releaseLogAfter: true);
					return true;
				}
			}
		}
		return false;
	}

	private void LeaveWurmHoles(LocationGridTile point1)
	{
		LocationGridTile locationGridTile = null;
		Region mainRegion = GridMap.Instance.mainRegion;
		for (int i = 0; i < 3; i++)
		{
			locationGridTile = mainRegion.GetRandomAreaThatIsNotWater().gridTileComponent.GetRandomPassableUnoccupiedNonWaterTile();
			if (locationGridTile != null)
			{
				break;
			}
		}
		if (point1 != null && locationGridTile != null)
		{
			InnerMapManager.Instance.CreateWurmHoles(point1, locationGridTile);
		}
	}

	private bool IsTamedMonster(Character p_character)
	{
		if (p_character is Summon summon)
		{
			return summon.isTamed;
		}
		return false;
	}
}
