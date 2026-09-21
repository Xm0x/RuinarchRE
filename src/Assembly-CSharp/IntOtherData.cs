public class IntOtherData : OtherData
{
	public int integer { get; }

	public override object obj => integer;

	public IntOtherData(int integer)
	{
		this.integer = integer;
	}

	public IntOtherData(SaveDataIntOtherData saveData)
	{
		integer = saveData.integer;
	}

	public override SaveDataOtherData Save()
	{
		SaveDataIntOtherData saveDataIntOtherData = new SaveDataIntOtherData();
		saveDataIntOtherData.Save(this);
		return saveDataIntOtherData;
	}

	public override void CleanUp()
	{
	}
}
