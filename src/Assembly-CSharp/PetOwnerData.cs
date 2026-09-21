public class PetOwnerData
{
	public Character petOwner { get; private set; }

	public void SetPetOwner(Character p_petOwner)
	{
		petOwner = p_petOwner;
	}

	public void LoadPetOwner(Character p_petOwner)
	{
		petOwner = p_petOwner;
	}
}
