using System.Collections.Generic;
using LapinerTools.Steam.Data;
using UnityEngine;

public class SWBContentPanel : MonoBehaviour
{
	[Header("Pooling")]
	[SerializeField]
	private Transform _poolParent;

	private Queue<SWBItem> _pooledItems = new Queue<SWBItem>(10);

	[SerializeField]
	private SWBItem m_leafNodePrefab;

	private List<SWBItem> _items = new List<SWBItem>();

	public void BuildTree(List<WorkshopItem> p_items)
	{
		if (m_leafNodePrefab != null)
		{
			for (int i = 0; i < p_items.Count; i++)
			{
				WorkshopItem workshopItem = p_items[i];
				if (workshopItem != null && !string.IsNullOrEmpty(workshopItem.Name))
				{
					SWBItem sWBItem = CreateNewItem();
					sWBItem.transform.SetParent(base.transform);
					sWBItem.SetData(workshopItem);
					_items.Add(sWBItem);
				}
			}
		}
		else
		{
			Debug.LogError("uMyGUI_TreeBrowser: BuildTree: you must provide the InnerNodePrefab and LeafNodePrefab in the inspector or via script!");
		}
	}

	public void Clear()
	{
		for (int i = 0; i < _items.Count; i++)
		{
			DestroyItem(_items[i]);
		}
		_items.Clear();
	}

	private SWBItem CreateNewItem()
	{
		if (_pooledItems.Count > 0)
		{
			return _pooledItems.Dequeue();
		}
		return Object.Instantiate(m_leafNodePrefab, _poolParent);
	}

	private void DestroyItem(SWBItem p_item)
	{
		p_item.transform.SetParent(_poolParent);
		p_item.transform.localPosition = Vector3.zero;
		p_item.ResetItem();
		_pooledItems.Enqueue(p_item);
	}

	public SWBItem GetActiveItem(ulong p_publishedField)
	{
		for (int i = 0; i < _items.Count; i++)
		{
			SWBItem sWBItem = _items[i];
			if (sWBItem.ContainsPublishedFileID(p_publishedField))
			{
				return sWBItem;
			}
		}
		return null;
	}
}
