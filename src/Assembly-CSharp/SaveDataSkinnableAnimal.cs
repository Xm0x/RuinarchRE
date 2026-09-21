public class SaveDataSkinnableAnimal : SaveDataSummon
{
	public int count;

	public bool isAvailableForShearing;

	public override void Save(Character character)
	{
		base.Save(character);
		SkinnableAnimal skinnableAnimal = character as SkinnableAnimal;
		count = skinnableAnimal.count;
	}

	public override Character Load()
	{
		SkinnableAnimal obj = base.Load() as SkinnableAnimal;
		obj.count = count;
		return obj;
	}
}
