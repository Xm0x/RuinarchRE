using System;
using System.Collections.Generic;
using Factions.Faction_Types;
using Locations.Settlements;
using UtilityScripts;

[Serializable]
public class FactionIdeology
{
	protected string _localizedName;

	protected string _localizedDescription;

	public FACTION_IDEOLOGY ideologyType { get; protected set; }

	public string name { get; protected set; }

	public int daysIntervalSettlementEvent { get; protected set; }

	public string localizedName
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedName))
			{
				_localizedName = LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", name);
			}
			return _localizedName;
		}
	}

	public string localizedDescription
	{
		get
		{
			if (string.IsNullOrEmpty(_localizedDescription))
			{
				_localizedDescription = LocalizationManager.Instance.GetLocalizedValue("FactionIdeologies_Table", name + "_Description");
			}
			return _localizedDescription;
		}
	}

	public FactionIdeology(FACTION_IDEOLOGY ideology)
	{
		ideologyType = ideology;
		name = Utilities.NormalizeStringUpperCaseFirstLetters(ideology.ToString());
	}

	public void InitializeFactionIdeology()
	{
	}

	public void SetSavedData(SaveDataFactionIdeology p_data)
	{
	}

	public virtual void LoadReferencesInMainThread(SaveDataFactionIdeology p_type)
	{
	}

	public virtual bool DoesCharacterFitIdeology(Character character)
	{
		return false;
	}

	public virtual bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return false;
	}

	public string GetIdeologyDescription()
	{
		return localizedDescription;
	}

	public string GetIdeologyDisplayName()
	{
		return localizedName;
	}

	protected virtual void OnAddIdeology(FactionType factionType, Faction p_faction)
	{
	}

	protected virtual void OnRemoveIdeology(FactionType p_factionType, Faction p_faction)
	{
	}

	public virtual bool IsReligionType()
	{
		return false;
	}

	protected virtual void SettlementEvent(BaseSettlement p_settlement)
	{
	}

	protected virtual void FactionMemberDied(Character p_deadCharacter)
	{
	}

	protected virtual bool CanCharacterDoIdeologyEventInternal(Character p_character)
	{
		return false;
	}

	public void TriggerIdeologySettlementEvent(BaseSettlement p_settlement)
	{
		SettlementEvent(p_settlement);
	}

	public void OnAddFactionIdeology(FactionType p_factionType, Faction p_faction)
	{
		OnAddIdeology(p_factionType, p_faction);
	}

	public void OnRemoveFactionIdeology(FactionType p_factionType, Faction p_faction)
	{
		List<BaseSettlement> ownedSettlements = p_faction.ownedSettlements;
		if (ownedSettlements != null && ownedSettlements.Count > 0)
		{
			for (int i = 0; i < ownedSettlements.Count; i++)
			{
				if (ownedSettlements[i] is NPCSettlement nPCSettlement)
				{
					nPCSettlement.factionIdeologyComponent.OnRemoveFactionIdeology(this, p_faction);
				}
			}
		}
		OnRemoveIdeology(p_factionType, p_faction);
	}

	public void OnFactionMemberDied(Character p_deadCharacter)
	{
		FactionMemberDied(p_deadCharacter);
	}

	public bool CanCharacterDoIdeologyEvent(Character p_character)
	{
		return CanCharacterDoIdeologyEventInternal(p_character);
	}
}
