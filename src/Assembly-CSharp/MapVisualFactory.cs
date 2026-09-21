using Inner_Maps;
using UnityEngine;

public class MapVisualFactory
{
	private static readonly string Tile_Object_Prefab_Name = "TileObjectGameObject";

	public GameObject CreateNewTileObjectMapVisual(TILE_OBJECT_TYPE objType)
	{
		switch (objType)
		{
		case TILE_OBJECT_TYPE.TORNADO:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoVisualObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.RAVENOUS_SPIRIT:
		case TILE_OBJECT_TYPE.FEEBLE_SPIRIT:
		case TILE_OBJECT_TYPE.FORLORN_SPIRIT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("SpiritGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.POISON_CLOUD:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PoisonCloudMapObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.LOCUST_SWARM:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("LocustSwarmMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.TORCH:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("TorchGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BED:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BedGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BALL_LIGHTNING:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BallLightningMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.CORN_CROP:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("CornCropMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.FROSTY_FOG:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("FrostyFogMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.VAPOR:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("VaporMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.FIRE_BALL:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("FireBallMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BERRY_SHRUB:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BerryShrubMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.QUICKSAND:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("QuicksandMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BRAZIER:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BrazierGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.FIREPLACE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("FireplaceGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BLOCK_WALL:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BlockWallGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.ICE_BLOCK_WALL:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("IceBlockWallGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.DOOR_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("DoorGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.EXCALIBUR:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("ExcaliburGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.CAMPFIRE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("CampfireGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.TABLE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("TableGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BED_CLINIC:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("ClinicBedGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.SMALL_TREE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("TreeGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BIG_TREE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BigTreeGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.ORE_VEIN:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("OreVeinGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.FISHING_SPOT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("FishingSpotGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PortalGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.WATCHER_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BeholderGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("ManaPitGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("BiolabGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.MARAUD_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("MaraudGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.SPIRE_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("SpireGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("ImpHutGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.PRISM_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PrismGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("MeddlerGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("CryptGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.DEMON_EYE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("EyeWardGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.DEFILER_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("DefilerGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("TortureChambersGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.KENNEL_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("KennelGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.HYPNO_HERB_CROP:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("HypnoHerbCropMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.ICEBERRY_CROP:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("IceberryCropMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.PINEAPPLE_CROP:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PineappleCropMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.POTATO_CROP:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PotatoCropMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.BOAR_DEN:
		case TILE_OBJECT_TYPE.WOLF_DEN:
		case TILE_OBJECT_TYPE.BEAR_DEN:
		case TILE_OBJECT_TYPE.RABBIT_HOLE:
		case TILE_OBJECT_TYPE.MINK_HOLE:
		case TILE_OBJECT_TYPE.MOONCRAWLER_HOLE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("AnimalBurrowGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.PRIMORDIAL_POOL_TILE_OBJECT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("PrimordialPoolGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.MONSTER_SPAWNER:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("MonsterSpawnerGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.DIVINE_ORB:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("DivineOrbGameObject", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.STAMPEDE:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("StampedeMapObjectVisual", Vector3.zero, Quaternion.identity);
		case TILE_OBJECT_TYPE.WARD_LIGHT:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("WardLightGameObject", Vector3.zero, Quaternion.identity);
		default:
			return ObjectPoolManager.Instance.InstantiateObjectFromPool(Tile_Object_Prefab_Name, Vector3.zero, Quaternion.identity);
		}
	}

	public CharacterVisionTrigger CreateAndInitializeCharacterVisionTrigger(Character p_character)
	{
		GameObject gameObject = ((p_character.race != RACE.DRAGON) ? Object.Instantiate(InnerMapManager.Instance.characterCollisionTriggerPrefab, p_character.marker.transform) : Object.Instantiate(InnerMapManager.Instance.dragonCollisionTriggerPrefab, p_character.marker.transform));
		gameObject.transform.localPosition = Vector3.zero;
		CharacterVisionTrigger component = gameObject.GetComponent<CharacterVisionTrigger>();
		component.Initialize(p_character);
		return component;
	}

	public GameObject CreateCharacterMarker(RACE p_race)
	{
		if (p_race == RACE.SPIRIT)
		{
			return ObjectPoolManager.Instance.InstantiateObjectFromPool("NatureSpiritCharacterMarker", Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
		}
		return ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterMarker", Vector3.zero, Quaternion.identity, InnerMapManager.Instance.transform);
	}
}
