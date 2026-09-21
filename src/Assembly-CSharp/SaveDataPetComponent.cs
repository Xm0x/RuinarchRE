using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataPetComponent : SaveData<PetComponent>
{
	public List<string> ownedPets;

	public string wyvernPet;

	public string petOwner;

	public int baseMaxPetCapacity;

	public override void Save(PetComponent data)
	{
		base.Save(data);
		if (data.ownedPets != null)
		{
			ownedPets = SaveUtilities.ConvertSavableListToIDs(data.ownedPets);
		}
		wyvernPet = data.wyvernPet?.persistentID;
		if (data.petOwner != null)
		{
			petOwner = data.petOwner.persistentID;
		}
		baseMaxPetCapacity = data.ownedPetsData.baseMaxPetCapacity;
	}

	public override PetComponent Load()
	{
		return new PetComponent();
	}

	public override void CleanUp()
	{
		if (ownedPets != null)
		{
			RuinarchListPool<string>.Release(ownedPets);
			ownedPets = null;
		}
	}
}
