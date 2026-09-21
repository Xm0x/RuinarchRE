using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class WorldConfigManager : MonoBehaviour
{
	public static WorldConfigManager Instance;

	[Header("Item Generation")]
	public ItemGenerationSetting worldWideItemGenerationSetting;

	public List<TILE_OBJECT_TYPE> initialArtifactChoices;

	[Header("Testing")]
	[SerializeField]
	private bool _disableLogs;

	public STRUCTURE_TYPE[] worldGenSpecialStructureChoices;

	public MapGenerationData mapGenerationData;

	private bool disableLogs => _disableLogs;

	private void Awake()
	{
		if (disableLogs)
		{
			Debug.unityLogger.logEnabled = false;
		}
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			Object.Destroy(Instance.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	[ContextMenu("Add Default Special Structures to World Gen Choices")]
	public void AddDefaultSpecialStructures()
	{
		STRUCTURE_TYPE[] enumValues = CollectionUtilities.GetEnumValues<STRUCTURE_TYPE>();
		List<STRUCTURE_TYPE> list = new List<STRUCTURE_TYPE>();
		foreach (STRUCTURE_TYPE sTRUCTURE_TYPE in enumValues)
		{
			if (sTRUCTURE_TYPE != STRUCTURE_TYPE.CAVE && sTRUCTURE_TYPE != STRUCTURE_TYPE.NECROMANCER_LAIR && sTRUCTURE_TYPE != STRUCTURE_TYPE.MUSHROOM_HAVEN && sTRUCTURE_TYPE != STRUCTURE_TYPE.LICH_GRAVEYARD && sTRUCTURE_TYPE.IsSpecialStructure())
			{
				list.Add(sTRUCTURE_TYPE);
			}
		}
		worldGenSpecialStructureChoices = list.ToArray();
	}
}
