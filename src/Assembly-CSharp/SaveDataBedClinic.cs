using System.Collections.Generic;

public class SaveDataBedClinic : SaveDataTileObject
{
	public List<string> userIDS;

	public override void Save(TileObject data)
	{
		base.Save(data);
		userIDS = new List<string>();
		BedClinic bedClinic = data as BedClinic;
		for (int i = 0; i < bedClinic.users.Length; i++)
		{
			Character character = bedClinic.users[i];
			if (character != null)
			{
				userIDS.Add(character.persistentID);
			}
		}
	}
}
