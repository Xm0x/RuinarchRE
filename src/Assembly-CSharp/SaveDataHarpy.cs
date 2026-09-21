using System;

[Serializable]
public class SaveDataHarpy : SaveDataSummon
{
	public bool hasCapturedForTheDay;

	public GameDate nextCaptureDate;

	public override void Save(Character data)
	{
		base.Save(data);
		if (data is Harpy harpy)
		{
			hasCapturedForTheDay = harpy.hasCapturedForTheDay;
			nextCaptureDate = harpy.nextCaptureDate;
		}
	}
}
