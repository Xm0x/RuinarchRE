using Inner_Maps.Location_Structures;

public class EventsData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EVENTS;

	public override string name => "Events";

	public EventsData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.STRUCTURE };
	}

	public override void ActivateAbility(LocationStructure targetStructure)
	{
		base.ActivateAbility(targetStructure);
		UIManager.Instance.ShowPrismEventsUI();
	}
}
