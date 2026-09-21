using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class DeadGroundsStructureObject : LocationStructureObject
{
	private readonly string[] _classChoices = new string[4] { "Barbarian", "Archer", "Noble", "Farmer" };

	protected override void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj, LocationGridTile tile, LocationStructure structure, TileObject newTileObject)
	{
		if (newTileObject is Tombstone tombstone)
		{
			tombstone.SetRespawnCorpseOnDestroy(state: false);
			tombstone.SetIsPreplaced(state: true);
			Character character = CharacterManager.Instance.CreateNewCharacter(CollectionUtilities.GetRandomElement(_classChoices), GameUtilities.RollChance(50) ? RACE.HUMANS : RACE.ELVES, (!GameUtilities.RollChance(50)) ? GENDER.FEMALE : GENDER.MALE, homeRegion: structure.region, faction: FactionManager.Instance.vagrantFaction, homeLocation: null, homeStructure: null, randomizeTraits: false);
			character.CreateMarker();
			character.InitialCharacterPlacement(tile);
			character.marker.UpdatePosition();
			character.Death("normal", null, null, null, null, null, null, isPlayerSource: false, this);
			tombstone.SetCharacter(character);
		}
		base.PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
	}
}
