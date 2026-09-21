public class BoneGolemData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BONE_GOLEM;

	public override string name => "Bone Golem";

	public override string description => "Bone Golem";

	public BoneGolemData()
	{
		base.summonType = SUMMON_TYPE.Bone_Golem;
		base.race = RACE.GOLEM;
		base.className = "Bone Golem";
	}
}
