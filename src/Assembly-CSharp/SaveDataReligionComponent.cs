using System.Collections.Generic;

public class SaveDataReligionComponent : SaveData<ReligionComponent>
{
	public RELIGION religion;

	public Dictionary<RELIGION, int> beliefPoints;

	public override void Save(ReligionComponent data)
	{
		religion = data.religion;
		if (data.beliefPoints != null && data.beliefPoints.Count > 0)
		{
			beliefPoints = new Dictionary<RELIGION, int>(data.beliefPoints);
		}
	}

	public override ReligionComponent Load()
	{
		return new ReligionComponent(this);
	}

	public override void CleanUp()
	{
		beliefPoints?.Clear();
		beliefPoints = null;
	}
}
