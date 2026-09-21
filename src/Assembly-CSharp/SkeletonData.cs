public class SkeletonData : SummonPlayerSkill
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SKELETON;

	public override string name => "Skeleton";

	public override string description => "Skeleton";

	public SkeletonData()
	{
		base.summonType = SUMMON_TYPE.Skeleton;
		base.race = RACE.SKELETON;
		base.className = "Skeleton";
	}
}
