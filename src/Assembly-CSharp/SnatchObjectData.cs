public class SnatchObjectData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH_OBJECT;

	public override string name => "Snatch Object";

	public override string description => "Snatch an object and bring them to a specific structure.";

	public override bool canBeCastOnBlessed => true;

	public override int radius => 2;

	public SnatchObjectData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override bool CanPerformAbilityTowards(TileObject target)
	{
		bool flag = !target.isTargetted && !target.partyComponent.HasSnatchPartyTargetingThis() && target.gridTileLocation != null;
		return base.CanPerformAbilityTowards(target) && flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = false;
		if (target is TileObject tileObject)
		{
			flag = tileObject.gridTileLocation != null || tileObject.isBeingCarriedBy != null;
			if (flag)
			{
				flag = !tileObject.isTargetted && !tileObject.partyComponent.HasSnatchPartyTargetingThis();
				if (flag)
				{
					flag = tileObject.Advertises(INTERACTION_TYPE.DEMON_STEAL);
				}
			}
		}
		return flag;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject target)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(target);
		if (target.isTargetted || target.partyComponent.HasSnatchPartyTargetingThis())
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Snatch_Object_Already_Targeted") + "|";
		}
		return text;
	}

	public override void ActivateAbility(IPointOfInterest target)
	{
		if (target is TileObject p_targetTileObject)
		{
			UIManager.Instance.ShowSnatchObjectUI(p_targetTileObject);
		}
	}
}
