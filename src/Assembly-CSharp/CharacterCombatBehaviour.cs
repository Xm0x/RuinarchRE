using UtilityScripts;

public class CharacterCombatBehaviour
{
	private string _description;

	public CHARACTER_COMBAT_BEHAVIOUR behaviourType { get; private set; }

	public string name { get; private set; }

	public string description
	{
		get
		{
			if (string.IsNullOrEmpty(_description))
			{
				_description = LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", name + "_Combat_Description");
			}
			return _description;
		}
	}

	public string displayName => LocalizationManager.Instance.GetLocalizedValue("CharacterCombat_Table", behaviourType.ToStringEnum());

	public CharacterCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_type)
	{
		behaviourType = p_type;
		name = Utilities.NotNormalizedConversionEnumToString(behaviourType.ToStringEnum());
	}

	public virtual void SetAsCombatBehaviourOf(Character p_character)
	{
	}

	public virtual void UnsetAsCombatBehaviourOf(Character p_character)
	{
	}

	public virtual void OnCharacterJoinedPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType)
	{
	}

	public virtual void OnCharacterLeftPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType)
	{
	}

	public virtual bool DetermineCombatBehaviour(Character p_character, CombatState p_combatState)
	{
		return false;
	}
}
