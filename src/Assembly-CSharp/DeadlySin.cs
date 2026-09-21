using System.Linq;

public class DeadlySin
{
	public DEADLY_SIN_ACTION[] assignments { get; protected set; }

	public bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction)
	{
		return assignments.Contains(sinAction);
	}

	public virtual PLAYER_SKILL_CATEGORY GetInterventionAbilityCategory()
	{
		return PLAYER_SKILL_CATEGORY.NONE;
	}
}
