public class SaveDataSharedOpinionModifier : SaveData<SharedOpinionModifier>, ISavableCounterpart
{
	public int modifierValue;

	public string targetCharacter;

	public string persistentID { get; private set; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Shared_Opinion_Modifier;

	public override void Save(SharedOpinionModifier data)
	{
		base.Save(data);
		persistentID = data.persistentID;
		modifierValue = data.modifierValue;
		targetCharacter = data.targetCharacter.persistentID;
	}

	public override void CleanUp()
	{
	}
}
