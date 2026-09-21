using System.Collections.Generic;

public class SaveDataResourceStorageComponent : SaveData<ResourceStorageComponent>
{
	public Dictionary<RESOURCE, int> storedResources;

	public Dictionary<CONCRETE_RESOURCES, int> specificStoredResources;

	public override void Save(ResourceStorageComponent data)
	{
		base.Save(data);
		storedResources = data.storedResources;
		specificStoredResources = data.specificStoredResources;
	}

	public override ResourceStorageComponent Load()
	{
		return new ResourceStorageComponent(this);
	}

	public override void CleanUp()
	{
		storedResources = null;
		specificStoredResources = null;
	}
}
