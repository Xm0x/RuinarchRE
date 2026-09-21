using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SaveDataHospice : SaveDataManMadeStructure
{
	public string[] beds;

	public List<string> bannedCharacters;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		Hospice hospice = locationStructure as Hospice;
		if (hospice.beds.Count > 0)
		{
			beds = new string[hospice.beds.Count];
			for (int i = 0; i < hospice.beds.Count; i++)
			{
				BedClinic bedClinic = hospice.beds[i];
				beds[i] = bedClinic.persistentID;
			}
		}
		bannedCharacters = SaveUtilities.ConvertSavableListToIDs(hospice.bannedCharacters);
	}
}
