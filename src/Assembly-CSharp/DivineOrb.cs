using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class DivineOrb : TileObject
{
	private InnerMapLight m_innerMapLight;

	public InnerMapLight InnerMap
	{
		get
		{
			if (m_innerMapLight == null)
			{
				m_innerMapLight = baseMapObjectVisual.GetComponentInChildren<InnerMapLight>(includeInactive: true);
			}
			return m_innerMapLight;
		}
	}

	public DivineOrb()
	{
		Initialize(TILE_OBJECT_TYPE.DIVINE_ORB);
	}

	public DivineOrb(SaveDataTileObject data)
		: base(data)
	{
	}

	protected override void OnPlaceTileObjectAtTile(LocationGridTile tile)
	{
		base.OnPlaceTileObjectAtTile(tile);
		if (tile.structure is ManMadeStructure { structureObjectHasOwnLight: not false })
		{
			DisableInnerMapLight();
		}
		else
		{
			EnableInnerMapLight();
		}
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	protected override void OnSetObjectAsBuilt()
	{
		base.OnSetObjectAsBuilt();
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (!actor.partyComponent.isMemberThatJoinedQuest && CharacterManager.Instance.GetVillagerWantInstance<HomeLightingWant>().GetFurnitureWanted(actor) == TILE_OBJECT_TYPE.DIVINE_ORB)
		{
			TryCreateObtainFurnitureWantOnReactionJob<HomeLightingWant>(actor);
		}
	}

	private void EnableInnerMapLight()
	{
		if (InnerMap != null)
		{
			InnerMap.gameObject.SetActive(value: true);
		}
	}

	private void DisableInnerMapLight()
	{
		if (InnerMap != null)
		{
			InnerMap.gameObject.SetActive(value: false);
		}
	}
}
