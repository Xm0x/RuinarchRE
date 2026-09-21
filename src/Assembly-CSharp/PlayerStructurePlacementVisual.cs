using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class PlayerStructurePlacementVisual : MonoBehaviour
{
	[SerializeField]
	private Transform transformInactiveStructuresParent;

	[SerializeField]
	private Transform transformColorTint;

	[SerializeField]
	private SpriteRenderer spriteRendererColorTint;

	[SerializeField]
	private Transform transformActiveStructuresParent;

	private Dictionary<STRUCTURE_TYPE, LocationStructureObject> _structureVisuals;

	private bool _isShowing;

	private LocationStructureObject _goActiveStructure;

	private bool _followMouse;

	private const float Tile_Size = 0.14f;

	public void Initialize(Camera p_camera)
	{
		List<STRUCTURE_TYPE> list = new List<STRUCTURE_TYPE>
		{
			STRUCTURE_TYPE.THE_PORTAL,
			STRUCTURE_TYPE.WATCHER,
			STRUCTURE_TYPE.BIOLAB,
			STRUCTURE_TYPE.DEFILER,
			STRUCTURE_TYPE.TORTURE_CHAMBERS,
			STRUCTURE_TYPE.KENNEL,
			STRUCTURE_TYPE.MEDDLER,
			STRUCTURE_TYPE.SPIRE,
			STRUCTURE_TYPE.MANA_PIT,
			STRUCTURE_TYPE.MARAUD,
			STRUCTURE_TYPE.PRISM,
			STRUCTURE_TYPE.CRYPT,
			STRUCTURE_TYPE.IMP_HUT,
			STRUCTURE_TYPE.PRIMORDIAL_POOL
		};
		_structureVisuals = new Dictionary<STRUCTURE_TYPE, LocationStructureObject>();
		transformColorTint.gameObject.SetActive(value: false);
		for (int i = 0; i < list.Count; i++)
		{
			STRUCTURE_TYPE sTRUCTURE_TYPE = list[i];
			GameObject firstStructurePrefabForStructure = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(FACTION_TYPE.Demons, new StructureSetting(sTRUCTURE_TYPE, RESOURCE.NONE));
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(firstStructurePrefabForStructure.name, Vector3.zero, Quaternion.identity, transformInactiveStructuresParent);
			LocationStructureObject component = obj.GetComponent<LocationStructureObject>();
			component.SetVisualMode(LocationStructureObject.Structure_Visual_Mode.Demonic_Structure_Blueprint, null);
			component.OverrideDefaultSortingOrder(1000);
			_structureVisuals.Add(sTRUCTURE_TYPE, component);
			obj.SetActive(value: false);
		}
	}

	public void Show(STRUCTURE_TYPE p_structureType)
	{
		_isShowing = true;
		_goActiveStructure = GetStructureVisual(p_structureType);
		_goActiveStructure.transform.SetParent(transformActiveStructuresParent);
		_goActiveStructure.transform.localPosition = Vector3.zero;
		_goActiveStructure.gameObject.SetActive(value: true);
		transformColorTint.localScale = new Vector3((float)_goActiveStructure.size.x * 0.14f, (float)_goActiveStructure.size.y * 0.14f, 0f);
		Vector3 zero = Vector3.zero;
		if (Utilities.IsEven(_goActiveStructure.size.x))
		{
			zero.x = -0.5f;
		}
		if (Utilities.IsEven(_goActiveStructure.size.y))
		{
			zero.y = -0.5f;
		}
		transformColorTint.transform.localPosition = zero;
		transformColorTint.gameObject.SetActive(value: true);
		SetFollowMouseState(p_state: true);
	}

	public void Hide()
	{
		_isShowing = false;
		_goActiveStructure.gameObject.SetActive(value: false);
		_goActiveStructure.transform.SetParent(transformInactiveStructuresParent);
		transformColorTint.gameObject.SetActive(value: false);
		SetFollowMouseState(p_state: false);
	}

	private LocationStructureObject GetStructureVisual(STRUCTURE_TYPE p_structureType)
	{
		return _structureVisuals[p_structureType];
	}

	public void SetFollowMouseState(bool p_state)
	{
		_followMouse = p_state;
	}

	public void SetHighlightColor(Color p_color)
	{
		spriteRendererColorTint.color = p_color;
	}

	private void Update()
	{
		if (_isShowing && _followMouse)
		{
			LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
			if (tileFromMousePosition != null)
			{
				transformActiveStructuresParent.transform.position = tileFromMousePosition.centeredWorldLocation;
			}
		}
	}
}
