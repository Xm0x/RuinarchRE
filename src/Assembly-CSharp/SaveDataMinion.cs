using System;

[Serializable]
public class SaveDataMinion : SaveData<Minion>
{
	public bool isSummoned;

	public PLAYER_SKILL_TYPE minionPlayerSkillType;

	public MINION_TYPE minionType;

	public override void Save(Minion minion)
	{
		isSummoned = minion.isSummoned;
		minionPlayerSkillType = minion.minionPlayerSkillType;
		minionType = minion.minionType;
	}

	public Minion Load(Character character)
	{
		return CharacterManager.Instance.CreateNewMinion(character, this);
	}
}
