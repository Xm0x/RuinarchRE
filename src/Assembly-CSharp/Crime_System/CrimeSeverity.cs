namespace Crime_System;

public class CrimeSeverity
{
	private string _localizedName;

	public string name { get; private set; }

	public CRIME_SEVERITY severity { get; private set; }

	public string localizedName
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedName))
			{
				_localizedName = LocalizationManager.Instance.GetLocalizedValue("CharacterCrimeSystem_Table", name + "_Crime_Severity");
			}
			return _localizedName;
		}
	}

	public CrimeSeverity(CRIME_SEVERITY severity)
	{
		this.severity = severity;
		name = severity.ToStringEnumWithSpace();
	}

	public string EffectAndReaction(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		Effect(witness, actor, target, crimeType, crime, reactionStatus);
		return Reaction(witness, actor, target, crimeType, crime, reactionStatus);
	}

	public virtual void Effect(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
	}

	public virtual string Reaction(Character witness, Character actor, IPointOfInterest target, CrimeType crimeType, ICrimeable crime, REACTION_STATUS reactionStatus)
	{
		return string.Empty;
	}
}
