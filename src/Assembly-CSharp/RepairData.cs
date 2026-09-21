using Inner_Maps.Location_Structures;

public class RepairData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.REPAIR;

	public override string name => "Repair";

	public override string description => "This Ability can be used to repair Demonic Structure damage.";

	public RepairData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure structure)
	{
		if (structure.currentHP < structure.maxHP)
		{
			if (structure is DemonicStructure demonicStructure)
			{
				demonicStructure.RepairStructure();
			}
			base.ActivateAbility(structure);
		}
	}

	public override bool CanPerformAbilityTowards(LocationStructure structure)
	{
		bool flag = base.CanPerformAbilityTowards(structure);
		if (flag)
		{
			if (structure is DemonicStructure)
			{
				return structure.currentHP < structure.maxHP;
			}
			return false;
		}
		return flag;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is ThePortal)
		{
			return false;
		}
		if (target is LocationStructure locationStructure && (locationStructure.hasBeenDestroyed || locationStructure.tiles.Count <= 0 || locationStructure.currentHP >= locationStructure.maxHP))
		{
			return false;
		}
		return base.IsValid(target);
	}
}
