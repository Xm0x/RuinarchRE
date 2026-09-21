public abstract class SaveDataOtherData : SaveData<OtherData>
{
	public int actionReferenceCount;

	public override void Save(OtherData data)
	{
		base.Save(data);
		actionReferenceCount = data.actionReferenceCount;
	}
}
