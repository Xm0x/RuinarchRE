public class SaveDataStringOtherData : SaveDataOtherData
{
	public string str;

	public override void Save(OtherData data)
	{
		base.Save(data);
		StringOtherData stringOtherData = data as StringOtherData;
		str = stringOtherData.str;
	}

	public override OtherData Load()
	{
		return new StringOtherData(this);
	}
}
