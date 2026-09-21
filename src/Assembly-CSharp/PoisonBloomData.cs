using Inner_Maps;
using Locations.Area_Features;

public class PoisonBloomData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.POISON_BLOOM;

	public override string name => "Poison Bloom";

	public override string description => "Random spots in the ground will start emitting small Poison Clouds that move around and then dissipates.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public PoisonBloomData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.AREA };
	}

	public override void ActivateAbility(Area targetArea)
	{
		(targetArea.featureComponent.AddFeature(AreaFeatureDB.Poison_Bloom_Feature, targetArea) as PoisonBloomFeature).SetIsPlayerSource(p_state: true);
		base.ActivateAbility(targetArea);
	}

	public override bool CanPerformAbilityTowards(Area targetArea)
	{
		bool flag = base.CanPerformAbilityTowards(targetArea);
		if (flag)
		{
			if (targetArea != null)
			{
				return !targetArea.featureComponent.HasFeature(AreaFeatureDB.Poison_Bloom_Feature);
			}
			return false;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(tile.area);
	}
}
