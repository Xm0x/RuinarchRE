using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Generation Setting", menuName = "Scriptable Objects/Item Generation")]
public class ItemGenerationSetting : ScriptableObject
{
	[SerializeField]
	private IntRange _iterations;

	[SerializeField]
	private BiomeItemDictionary _itemChoices;

	public IntRange iterations => _iterations;

	public List<ItemSetting> GetItemChoicesForBiome()
	{
		List<ItemSetting> list = new List<ItemSetting>();
		if (_itemChoices.ContainsKey(BIOMES.NONE))
		{
			list.AddRange(_itemChoices[BIOMES.NONE]);
		}
		return list;
	}
}
