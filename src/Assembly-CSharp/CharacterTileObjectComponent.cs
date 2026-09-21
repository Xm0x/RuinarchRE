using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CharacterTileObjectComponent : CharacterComponent
{
	public Bed primaryBed { get; private set; }

	public BaseBed bedBeingUsed { get; private set; }

	public bool isUsingBed => bedBeingUsed != null;

	public CharacterTileObjectComponent()
	{
	}

	public CharacterTileObjectComponent(SaveDataCharacterTileObjectComponent data)
	{
	}

	public void SetPrimaryBed(Bed bed)
	{
		primaryBed = bed;
	}

	public void SetBedBeingUsed(BaseBed p_bed)
	{
		bedBeingUsed = p_bed;
	}

	public void OnPlacedTileObjectInHomeStructure(LocationStructure p_structure, TileObject p_tileObject)
	{
		if (p_tileObject.mapObjectState != MAP_OBJECT_STATE.BUILT || primaryBed != null || !(p_tileObject is Bed bed))
		{
			return;
		}
		if (bed.characterOwner == null || bed.IsOwnedBy(base.owner))
		{
			SetPrimaryBed(bed);
			return;
		}
		bool flag = false;
		for (int i = 0; i < p_structure.residents.Count; i++)
		{
			Character character = p_structure.residents[i];
			if (character != base.owner && bed.IsOwnedBy(character))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			SetPrimaryBed(bed);
		}
	}

	public void DropRandomItemBasedOnWeights(Character p_responsibleCharacter)
	{
		TILE_OBJECT_TYPE randomWeightedItemDropByCharacter = CharacterManager.Instance.GetRandomWeightedItemDropByCharacter(base.owner, p_responsibleCharacter);
		if (randomWeightedItemDropByCharacter == TILE_OBJECT_TYPE.NONE)
		{
			return;
		}
		TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomWeightedItemDropByCharacter);
		LocationGridTile locationGridTile = base.owner.gridTileLocation;
		if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
		{
			locationGridTile = base.owner.gridTileLocation.GetFirstNeighborThatIsPassableAndNoObject();
		}
		if (locationGridTile != null)
		{
			locationGridTile.structure.AddPOI(tileObject, locationGridTile);
			if (tileObject is EquipmentItem equipmentItem)
			{
				equipmentItem.TryAddRandomPrefix();
			}
		}
	}

	public void LoadReferences(SaveDataCharacterTileObjectComponent data)
	{
		if (!string.IsNullOrEmpty(data.primaryBed))
		{
			primaryBed = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.primaryBed) as Bed;
		}
		if (!string.IsNullOrEmpty(data.bedBeingUsed))
		{
			bedBeingUsed = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.bedBeingUsed) as BaseBed;
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
