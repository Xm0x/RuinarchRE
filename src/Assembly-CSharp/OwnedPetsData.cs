using System.Collections.Generic;

public class OwnedPetsData
{
	public List<Character> pets { get; private set; }

	public Wyvern wyvernPet { get; private set; }

	public int baseMaxPetCapacity { get; private set; }

	public bool AddPet(Character p_pet)
	{
		if (pets == null)
		{
			pets = new List<Character>();
		}
		if (!pets.Contains(p_pet))
		{
			pets.Add(p_pet);
			if (p_pet is Wyvern wyvern)
			{
				wyvernPet = wyvern;
			}
			return true;
		}
		return false;
	}

	public bool RemovePet(Character p_pet)
	{
		if (pets != null && pets.Remove(p_pet))
		{
			if (p_pet == wyvernPet)
			{
				wyvernPet = null;
			}
			return true;
		}
		return false;
	}

	public void LoadPets(List<Character> p_pets, Character p_wyvernPet)
	{
		pets = p_pets;
		wyvernPet = p_wyvernPet as Wyvern;
	}

	public void SetBaseMaximumPetCapacity(int p_capacity)
	{
		baseMaxPetCapacity = p_capacity;
	}

	public bool IsAtMaxCapacity()
	{
		int num = baseMaxPetCapacity;
		if (pets == null || pets.Count <= 0)
		{
			if (num > 0)
			{
				return false;
			}
			return true;
		}
		return pets.Count >= num;
	}

	public bool HasWyvernPet()
	{
		return wyvernPet != null;
	}

	public Wyvern GetWyvernPet()
	{
		return wyvernPet;
	}
}
