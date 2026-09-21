using Inner_Maps.Location_Structures;

public class SaveDataPrisonCell : SaveDataStructureRoom
{
	public string tortureID;

	public string brainwashTargetID;

	public override void Save(StructureRoom data)
	{
		base.Save(data);
		PrisonCell prisonCell = data as PrisonCell;
		tortureID = ((prisonCell.currentTortureTarget == null) ? string.Empty : prisonCell.currentTortureTarget.persistentID);
		brainwashTargetID = ((prisonCell.currentBrainwashTarget == null) ? string.Empty : prisonCell.currentBrainwashTarget.persistentID);
	}
}
