using System;
using System.Collections.Generic;

[Serializable]
public class SaveDataDragon : SaveDataSkinnableAnimal
{
	public bool isAwakened;

	public bool isAttackingPlayer;

	public bool willLeaveWorld;

	public int leaveWorldCounter;

	public List<string> charactersThatAreWary;

	public string targetStructure;

	public override void Save(Character data)
	{
		base.Save(data);
		if (!(data is Dragon dragon))
		{
			return;
		}
		isAwakened = dragon.isAwakened;
		isAttackingPlayer = dragon.isAttackingPlayer;
		willLeaveWorld = dragon.willLeaveWorld;
		leaveWorldCounter = dragon.leaveWorldCounter;
		if (dragon.targetStructure != null)
		{
			targetStructure = dragon.targetStructure.persistentID;
		}
		if (dragon.charactersThatAreWary.Count > 0)
		{
			charactersThatAreWary = new List<string>();
			for (int i = 0; i < dragon.charactersThatAreWary.Count; i++)
			{
				charactersThatAreWary.Add(dragon.charactersThatAreWary[i].persistentID);
			}
		}
	}
}
