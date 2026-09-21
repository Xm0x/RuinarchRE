public class CultLeaderEvent : PrismEvent
{
	public int numberOfAliveDemonCultists { get; private set; }

	public int numberOfAliveCultLeaders { get; private set; }

	public int retaliationMeter { get; private set; }

	public CultLeaderEvent(PrismEventData p_data)
		: base(p_data)
	{
		base.requirements = new PrismEventRequirement[3]
		{
			new PrismEventRequirement("Cult_Leader_Event_Requirement_1", IsCultistCountRequirementSatisfied),
			new PrismEventRequirement("Cult_Leader_Event_Requirement_2", IsCultLeaderCountRequirementSatisfied),
			new PrismEventRequirement("Cult_Leader_Event_Requirement_3", IsRetaliationMeterRequirementSatisfied)
		};
	}

	public void AdjustNumberOfAliveDemonCultists(int p_amount)
	{
		numberOfAliveDemonCultists += p_amount;
	}

	public void SetNumberOfAliveDemonCultists(int p_amount)
	{
		numberOfAliveDemonCultists = p_amount;
	}

	public void AdjustNumberOfAliveCultLeaders(int p_amount)
	{
		numberOfAliveCultLeaders += p_amount;
	}

	public void SetNumberOfAliveCultLeaders(int p_amount)
	{
		numberOfAliveCultLeaders = p_amount;
	}

	public void SetRetaliationMeter(int p_amount)
	{
		retaliationMeter = p_amount;
	}

	private void TurnADemonCultistIntoALeader()
	{
		Character character = null;
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character2 = CharacterManager.Instance.allCharacters[i];
			if (!character2.isDead && !character2.isInLimbo && character2.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship))
			{
				character = character2;
				break;
			}
		}
		string cultLeaderClassNameForReligion = RELIGION.Demon_Worship.GetCultLeaderClassNameForReligion();
		character.classComponent.AssignClass(cultLeaderClassNameForReligion);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupts", "Interrupts_Table", "Become Cult Leader effect", LOG_TAG.Major);
		log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance?.player?.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private bool IsCultistCountRequirementSatisfied()
	{
		return numberOfAliveDemonCultists >= 5;
	}

	private bool IsCultLeaderCountRequirementSatisfied()
	{
		return numberOfAliveCultLeaders <= 0;
	}

	private bool IsRetaliationMeterRequirementSatisfied()
	{
		return retaliationMeter <= 0;
	}

	protected override void TriggerEventBase()
	{
		base.TriggerEventBase();
		TurnADemonCultistIntoALeader();
	}
}
