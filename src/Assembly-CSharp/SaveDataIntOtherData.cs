public class SaveDataIntOtherData : SaveDataOtherData
{
	public int integer;

	public override void Save(OtherData data)
	{
		base.Save(data);
		IntOtherData intOtherData = data as IntOtherData;
		integer = intOtherData.integer;
	}

	public override OtherData Load()
	{
		return new IntOtherData(this);
	}
}
