using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SaveDataWatcher : SaveDataDemonicStructure
{
	public List<string> eyeWards;

	public int eyesLevel;

	public int radiusLevel;

	public int eyeWardMaxCount;

	public int eyeWardRadius;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		Watcher watcher = structure as Watcher;
		if (watcher.eyeWards.Count > 0)
		{
			eyeWards = new List<string>();
			for (int i = 0; i < watcher.eyeWards.Count; i++)
			{
				eyeWards.Add(watcher.eyeWards[i].persistentID);
			}
		}
		eyesLevel = watcher.GetEyeLevel();
		radiusLevel = watcher.GetRadiusLevel();
		eyeWardRadius = watcher.GetEyeWardRadius();
		eyeWardMaxCount = watcher.GetCurrentMaxEyeCount();
	}
}
