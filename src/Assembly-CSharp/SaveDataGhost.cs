using System;

[Serializable]
public class SaveDataGhost : SaveDataSummon
{
	public string betrayedBy;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Ghost { betrayedBy: not null } ghost)
		{
			betrayedBy = ghost.betrayedBy.persistentID;
		}
	}
}
