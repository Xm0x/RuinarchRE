public class StringOtherData : OtherData
{
	public string str { get; private set; }

	public override object obj => str;

	public StringOtherData(string str)
	{
		this.str = str;
	}

	public StringOtherData(SaveDataStringOtherData saveData)
	{
		str = saveData.str;
	}

	public void SetString(string p_value)
	{
		str = p_value;
	}

	public override SaveDataOtherData Save()
	{
		SaveDataStringOtherData saveDataStringOtherData = new SaveDataStringOtherData();
		saveDataStringOtherData.Save(this);
		return saveDataStringOtherData;
	}

	public override void CleanUp()
	{
		str = string.Empty;
	}
}
