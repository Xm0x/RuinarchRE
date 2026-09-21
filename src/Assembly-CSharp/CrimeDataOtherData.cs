using Inner_Maps.Location_Structures;

public class CrimeDataOtherData : OtherData
{
	public CrimeData crimeData { get; private set; }

	public override object obj => crimeData;

	public CrimeDataOtherData(CrimeData crimeData)
	{
		this.crimeData = crimeData;
	}

	public CrimeDataOtherData(SaveDataCrimeDataOtherData data)
	{
		crimeData = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.crimeDataID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataCrimeDataOtherData saveDataCrimeDataOtherData = new SaveDataCrimeDataOtherData();
		saveDataCrimeDataOtherData.Save(this);
		return saveDataCrimeDataOtherData;
	}

	public override void CleanUp()
	{
		crimeData = null;
	}

	public override bool IsCharacterReferenced(Character p_character)
	{
		bool flag = base.IsCharacterReferenced(p_character);
		if (!flag)
		{
			flag = crimeData.IsCharacterReferenced(p_character);
		}
		return flag;
	}

	public override bool IsStructureReferenced(LocationStructure p_structure)
	{
		bool flag = base.IsStructureReferenced(p_structure);
		if (!flag)
		{
			flag = crimeData.IsStructureReferenced(p_structure);
		}
		return flag;
	}

	public override bool IsOtherDataInvalid()
	{
		bool flag = base.IsOtherDataInvalid();
		if (!flag && crimeData != null)
		{
			flag = crimeData.IsCrimeDataInvalid();
		}
		return flag;
	}
}
