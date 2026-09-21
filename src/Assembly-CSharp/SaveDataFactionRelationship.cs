using System;

[Serializable]
public class SaveDataFactionRelationship : SaveData<FactionRelationship>
{
	public string faction1ID;

	public string faction2ID;

	public int relationshipStatInt;

	public override void Save(FactionRelationship data)
	{
		faction1ID = data.faction1.persistentID;
		faction2ID = data.faction2.persistentID;
		relationshipStatInt = data.relationshipStatInt;
	}

	public override FactionRelationship Load()
	{
		Faction factionByPersistentID = FactionManager.Instance.GetFactionByPersistentID(faction1ID);
		Faction factionByPersistentID2 = FactionManager.Instance.GetFactionByPersistentID(faction2ID);
		return new FactionRelationship(factionByPersistentID, factionByPersistentID2, relationshipStatInt);
	}
}
