using System;

[Serializable]
public class SaveDataSocialGathering : SaveDataGathering
{
	public string targetStructure;

	public override void Save(Gathering data)
	{
		base.Save(data);
		if (data is SocialGathering { targetStructure: not null } socialGathering)
		{
			targetStructure = socialGathering.targetStructure.persistentID;
		}
	}
}
