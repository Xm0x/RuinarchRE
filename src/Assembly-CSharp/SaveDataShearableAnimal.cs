public class SaveDataShearableAnimal : SaveDataSummon
{
	public int count;

	public bool isAvailableForShearing;

	public override void Save(Character character)
	{
		base.Save(character);
		ShearableAnimal shearableAnimal = character as ShearableAnimal;
		count = shearableAnimal.count;
		isAvailableForShearing = shearableAnimal.isAvailableForShearing;
	}

	public override Character Load()
	{
		ShearableAnimal obj = base.Load() as ShearableAnimal;
		obj.count = count;
		obj.isAvailableForShearing = isAvailableForShearing;
		return obj;
	}
}
