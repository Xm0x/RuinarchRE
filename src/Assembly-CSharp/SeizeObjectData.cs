public class SeizeObjectData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SEIZE_OBJECT;

	public override string name => "Seize Object";

	public override string description => "This Ability can be used to take an object and then transfer it to an unoccupied tile.\nTaking a resource pile from a Village city center will produce a Chaos Orb.";

	public SeizeObjectData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE_OBJECT };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		PlayerManager.Instance.player.seizeComponent.SeizePOI(targetPOI);
		base.ActivateAbility(targetPOI);
	}

	public override bool CanPerformAbilityTowards(TileObject targetTileObject)
	{
		if (base.CanPerformAbilityTowards(targetTileObject))
		{
			if (!targetTileObject.canBeSeized)
			{
				return false;
			}
			if (targetTileObject.traitContainer.HasTrait("Heavy"))
			{
				return false;
			}
			if (!PlayerManager.Instance.player.seizeComponent.hasSeizedPOI)
			{
				if (!(targetTileObject.mapVisual != null))
				{
					return targetTileObject.isBeingCarriedBy != null;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(TileObject targetTileObject)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetTileObject);
		if (targetTileObject.traitContainer.HasTrait("Heavy"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Seize_Heavy_Object") + "|";
		}
		return text;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		bool flag = base.IsValid(target);
		if (target is TileObject tileObject && (flag || tileObject.isBeingCarriedBy != null))
		{
			if (!tileObject.canBeSeized)
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
