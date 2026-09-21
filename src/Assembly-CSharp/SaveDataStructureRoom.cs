using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SaveDataStructureRoom : SaveData<StructureRoom>
{
	public string name;

	public List<string> tilesInRoom;

	public override void Save(StructureRoom data)
	{
		base.Save(data);
		name = data.name;
		tilesInRoom = SaveUtilities.ConvertSavableListToIDs(data.tilesInRoom);
	}

	public override void CleanUp()
	{
		if (tilesInRoom != null)
		{
			RuinarchListPool<string>.Release(tilesInRoom);
			tilesInRoom = null;
		}
	}
}
