namespace Crime_System;

public class CrimeType
{
	private string _localizedName;

	private string _localizedAccuseText;

	private string _localizedLastStrawText;

	public CRIME_TYPE type { get; private set; }

	public string name { get; private set; }

	public string localizedAccuseText
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedAccuseText))
			{
				_localizedAccuseText = LocalizationManager.Instance.GetLocalizedValue("CharacterCrimeSystem_Table", name + "_Accuse_Text");
				if (string.IsNullOrEmpty(_localizedAccuseText))
				{
					_localizedAccuseText = localizedName;
				}
			}
			return _localizedAccuseText;
		}
	}

	public string localizedName
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedName))
			{
				_localizedName = LocalizationManager.Instance.GetLocalizedValue("CharacterCrimeSystem_Table", name + "_Crime_Type");
			}
			return _localizedName;
		}
	}

	public string localizedLastStrawText
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedLastStrawText))
			{
				_localizedLastStrawText = LocalizationManager.Instance.GetLocalizedValue("CharacterCrimeSystem_Table", name + "_Last_Straw");
			}
			return _localizedLastStrawText;
		}
	}

	public CrimeType(CRIME_TYPE type)
	{
		this.type = type;
		name = type.ToStringEnumWithSpace();
	}

	public virtual CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target)
	{
		if (CharacterManager.Instance.IsCultistOfSameReligion(witness, actor))
		{
			if (target is TileObject tileObject)
			{
				if (!tileObject.IsOwnedBy(witness))
				{
					return CRIME_SEVERITY.None;
				}
			}
			else
			{
				if (!(target is Character character))
				{
					return CRIME_SEVERITY.None;
				}
				if (character != witness && !CharacterManager.Instance.IsCultistOfSameReligion(witness, character))
				{
					return CRIME_SEVERITY.None;
				}
			}
		}
		return CRIME_SEVERITY.Unapplicable;
	}

	public string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime)
	{
		return actor.name + " " + localizedLastStrawText;
	}

	public virtual void ProcessReactionOnAccuse(Character p_witness, Character p_actor, IPointOfInterest p_target)
	{
	}

	protected void StalkerReactionsToVampireLycanCultist(Character p_witness, Character p_actor, IPointOfInterest p_target)
	{
		p_witness.classComponent.StalkerHunt(p_actor);
	}
}
