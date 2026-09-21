using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;

public interface IDwelling
{
	int id { get; }

	string name { get; }

	STRUCTURE_TYPE structureType { get; }

	List<Character> charactersHere { get; }

	List<Character> residents { get; }

	Region location { get; }

	BaseSettlement settlementLocation { get; }

	HashSet<IPointOfInterest> pointsOfInterest { get; }

	POI_STATE state { get; }

	LocationStructureObject structureObj { get; }

	List<LocationGridTile> tiles { get; }

	LinkedList<LocationGridTile> unoccupiedTiles { get; }

	void AddResident(Character character);

	void RemoveResident(Character character);

	string GetNameRelativeTo(Character character);

	bool IsResident(Character character);

	bool IsOccupied();

	bool CanBeResidentHere(Character character);

	bool HasPositiveRelationshipWithAnyResident(Character character);

	bool HasEnemyOrNoRelationshipWithAnyResident(Character character);

	bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null, bool placeObject = true);

	bool RemovePOI(IPointOfInterest poi, Character removedBy = null);

	bool HasUnoccupiedFurnitureSpot();

	TileObject GetUnoccupiedTileObject(params TILE_OBJECT_TYPE[] type);

	List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type);

	T GetTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject;
}
