using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class ResourceStorageComponent
{
	private Dictionary<CONCRETE_RESOURCES, int> _specificStoredResourcesCopy;

	private Dictionary<RESOURCE, int> _storedResourcesCopy;

	public Dictionary<RESOURCE, int> storedResources { get; }

	public Dictionary<CONCRETE_RESOURCES, int> specificStoredResources { get; }

	public ResourceStorageComponent()
	{
		storedResources = new Dictionary<RESOURCE, int>();
		_specificStoredResourcesCopy = new Dictionary<CONCRETE_RESOURCES, int>();
		_storedResourcesCopy = new Dictionary<RESOURCE, int>();
		RESOURCE[] enumValues = CollectionUtilities.GetEnumValues<RESOURCE>();
		foreach (RESOURCE rESOURCE in enumValues)
		{
			if (rESOURCE != RESOURCE.NONE)
			{
				storedResources.Add(rESOURCE, 0);
			}
		}
		specificStoredResources = new Dictionary<CONCRETE_RESOURCES, int>();
		CONCRETE_RESOURCES[] enumValues2 = CollectionUtilities.GetEnumValues<CONCRETE_RESOURCES>();
		foreach (CONCRETE_RESOURCES key in enumValues2)
		{
			specificStoredResources.Add(key, 0);
		}
	}

	public ResourceStorageComponent(SaveDataResourceStorageComponent p_data)
	{
		if (p_data.storedResources != null && p_data.storedResources.Count > 0)
		{
			storedResources = new Dictionary<RESOURCE, int>(p_data.storedResources);
		}
		if (storedResources == null)
		{
			storedResources = new Dictionary<RESOURCE, int>();
			RESOURCE[] enumValues = CollectionUtilities.GetEnumValues<RESOURCE>();
			foreach (RESOURCE rESOURCE in enumValues)
			{
				if (rESOURCE != RESOURCE.NONE)
				{
					storedResources.Add(rESOURCE, 0);
				}
			}
		}
		if (p_data.specificStoredResources != null && p_data.specificStoredResources.Count > 0)
		{
			specificStoredResources = new Dictionary<CONCRETE_RESOURCES, int>(p_data.specificStoredResources);
		}
		if (specificStoredResources == null)
		{
			specificStoredResources = new Dictionary<CONCRETE_RESOURCES, int>();
			CONCRETE_RESOURCES[] enumValues2 = CollectionUtilities.GetEnumValues<CONCRETE_RESOURCES>();
			foreach (CONCRETE_RESOURCES key in enumValues2)
			{
				specificStoredResources.Add(key, 0);
			}
		}
		_specificStoredResourcesCopy = new Dictionary<CONCRETE_RESOURCES, int>();
		_storedResourcesCopy = new Dictionary<RESOURCE, int>();
	}

	private void SetResource(RESOURCE resourceType, int amount)
	{
		storedResources[resourceType] = amount;
		storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
	}

	private void AdjustResource(RESOURCE resourceType, int amount)
	{
		storedResources[resourceType] += amount;
		storedResources[resourceType] = Mathf.Max(storedResources[resourceType], 0);
	}

	public void ClearAllResources()
	{
		_storedResourcesCopy.Clear();
		foreach (KeyValuePair<RESOURCE, int> storedResource in storedResources)
		{
			_storedResourcesCopy.Add(storedResource.Key, storedResource.Value);
		}
		foreach (KeyValuePair<RESOURCE, int> item in _storedResourcesCopy)
		{
			storedResources[item.Key] = 0;
		}
		_specificStoredResourcesCopy.Clear();
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in specificStoredResources)
		{
			_specificStoredResourcesCopy.Add(specificStoredResource.Key, specificStoredResource.Value);
		}
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> item2 in _specificStoredResourcesCopy)
		{
			specificStoredResources[item2.Key] = 0;
		}
	}

	public void ReduceMainResourceUsingRandomSpecificResources(RESOURCE p_resource, int amount)
	{
		int num = amount;
		_specificStoredResourcesCopy.Clear();
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in specificStoredResources)
		{
			_specificStoredResourcesCopy.Add(specificStoredResource.Key, specificStoredResource.Value);
		}
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> item in _specificStoredResourcesCopy)
		{
			if (num <= 0)
			{
				break;
			}
			int value = item.Value;
			CONCRETE_RESOURCES key = item.Key;
			if (key.GetResourceCategory() == p_resource && value > 0)
			{
				int num2 = num;
				if (num2 > value)
				{
					num2 = value;
				}
				num -= num2;
				AdjustResource(key, -num2);
			}
		}
		_specificStoredResourcesCopy.Clear();
		_ = 0;
	}

	public void AdjustResource(CONCRETE_RESOURCES p_resource, int p_amount)
	{
		specificStoredResources[p_resource] += p_amount;
		specificStoredResources[p_resource] = Mathf.Max(specificStoredResources[p_resource], 0);
		RESOURCE resourceCategory = p_resource.GetResourceCategory();
		AdjustResource(resourceCategory, p_amount);
	}

	public void SetResource(CONCRETE_RESOURCES p_resource, int p_amount)
	{
		int num = specificStoredResources[p_resource];
		specificStoredResources[p_resource] = p_amount;
		specificStoredResources[p_resource] = Mathf.Max(specificStoredResources[p_resource], 0);
		int amount = specificStoredResources[p_resource] - num;
		RESOURCE resourceCategory = p_resource.GetResourceCategory();
		AdjustResource(resourceCategory, amount);
	}

	public bool HasResourceAmount(RESOURCE resourceType, int amount)
	{
		return storedResources[resourceType] >= amount;
	}

	public bool IsAtMaxResource(RESOURCE resource)
	{
		return false;
	}

	public bool HasEnoughSpaceFor(RESOURCE resource, int amount)
	{
		_ = storedResources[resource];
		return true;
	}

	public int GetResourceValue(RESOURCE resource)
	{
		return storedResources[resource];
	}

	public CONCRETE_RESOURCES GetFirstResourceWithValue()
	{
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in specificStoredResources)
		{
			if (specificStoredResource.Value > 0)
			{
				return specificStoredResource.Key;
			}
		}
		throw new Exception("Could not find a resource greater than 0");
	}

	public bool TryGetFirstResourceWithValue(out CONCRETE_RESOURCES p_resource)
	{
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in specificStoredResources)
		{
			if (specificStoredResource.Value > 0)
			{
				p_resource = specificStoredResource.Key;
				return true;
			}
		}
		p_resource = CONCRETE_RESOURCES.Copper;
		return false;
	}

	public void SpawnResourcesInStorage(LocationGridTile gridTileLocation)
	{
		foreach (KeyValuePair<CONCRETE_RESOURCES, int> specificStoredResource in specificStoredResources)
		{
			if (specificStoredResource.Value > 0)
			{
				LocationGridTile firstNearestTileFromThisWithNoObject = gridTileLocation.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
				if (firstNearestTileFromThisWithNoObject == null)
				{
					break;
				}
				TILE_OBJECT_TYPE tileObjectType = specificStoredResource.Key.ConvertResourcesToTileObjectType();
				ResourcePile resourcePile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(tileObjectType);
				firstNearestTileFromThisWithNoObject.structure.AddPOI(resourcePile, firstNearestTileFromThisWithNoObject);
				resourcePile.SetResourceInPile(specificStoredResource.Value);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
