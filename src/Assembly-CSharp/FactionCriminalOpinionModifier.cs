using System;

public class FactionCriminalOpinionModifier : SharedOpinionModifier
{
	public CRIME_TYPE crimeType { get; private set; }

	public CRIME_SEVERITY crimeSeverity { get; private set; }

	public override Type serializedData => typeof(SaveDataFactionCriminalOpinionModifier);

	public override string modifierName => crimeSeverity.ToStringEnumWithSpace() + " crime (" + crimeType.ToStringEnumWithSpace() + ")";

	public FactionCriminalOpinionModifier(CRIME_TYPE p_crime, CRIME_SEVERITY p_crimeSeverity, Character p_targetCharacter)
		: base(p_targetCharacter)
	{
		crimeType = p_crime;
		crimeSeverity = p_crimeSeverity;
		base.modifierValue = GetInitialRelationshipModificationValue(p_crimeSeverity);
		Messenger.AddListener(Signals.DAY_STARTED, DegradeModifierPerDay);
	}

	public FactionCriminalOpinionModifier(SaveDataFactionCriminalOpinionModifier data)
		: base(data)
	{
		crimeType = data.crimeType;
		crimeSeverity = data.crimeSeverity;
		Messenger.AddListener(Signals.DAY_STARTED, DegradeModifierPerDay);
	}

	private int GetInitialRelationshipModificationValue(CRIME_SEVERITY p_crimeSeverity)
	{
		return p_crimeSeverity switch
		{
			CRIME_SEVERITY.Serious => -40, 
			CRIME_SEVERITY.Heinous => -60, 
			_ => throw new Exception("No initial relationship modification value for " + p_crimeSeverity.ToStringEnumWithSpace()), 
		};
	}

	protected override void OnIncreaseModifierValue()
	{
		if (base.targetCharacter == null)
		{
			base.eventDispatcher.ExecuteModifierExpired(this);
			Messenger.RemoveListener(Signals.DAY_STARTED, DegradeModifierPerDay);
			return;
		}
		base.OnIncreaseModifierValue();
		if (base.modifierValue >= 0)
		{
			base.eventDispatcher.ExecuteModifierExpired(this);
			Messenger.RemoveListener(Signals.DAY_STARTED, DegradeModifierPerDay);
		}
	}

	public override void OnModifierRemovedFromDatabase()
	{
		base.OnModifierRemovedFromDatabase();
		base.targetCharacter = null;
		Messenger.RemoveListener(Signals.DAY_STARTED, DegradeModifierPerDay);
	}

	private void DegradeModifierPerDay()
	{
		IncreaseModifierValue(5);
	}
}
