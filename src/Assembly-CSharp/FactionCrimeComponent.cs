using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class FactionCrimeComponent : FactionComponent
{
	public List<Character> wantedCharacters { get; private set; }

	public FactionCrimeComponent()
	{
		wantedCharacters = new List<Character>();
		SubscribeListeners();
	}

	public FactionCrimeComponent(SaveDataFactionCrimeComponent data)
	{
		wantedCharacters = new List<Character>();
		SubscribeListeners();
	}

	public void LoadReferences(SaveDataFactionCrimeComponent data)
	{
		if (data.wantedCharacters != null)
		{
			wantedCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(data.wantedCharacters));
		}
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<Character, CrimeData>(FactionSignals.CRIME_REMOVED_FROM_CRIMINAL, OnCrimeRemovedFromCriminal);
		Messenger.AddListener<Faction, Character, CrimeData>(FactionSignals.BECOME_WANTED_CRIMINAL_OF_FACTION, OnCharacterBecameWantedByFaction);
	}

	private void OnCrimeRemovedFromCriminal(Character p_character, CrimeData p_crimeData)
	{
		if (!p_character.crimeComponent.IsWantedBy(base.owner) && wantedCharacters.Contains(p_character))
		{
			RemoveWantedCharacter(p_character);
		}
	}

	private void OnCharacterBecameWantedByFaction(Faction p_faction, Character p_criminal, CrimeData p_crimeData)
	{
		if (p_faction == base.owner)
		{
			AddWantedCharacter(p_criminal, p_crimeData);
		}
	}

	private void AddWantedCharacter(Character p_character, CrimeData p_crimeData)
	{
		if (!wantedCharacters.Contains(p_character))
		{
			wantedCharacters.Add(p_character);
			if (p_crimeData.crimeSeverity == CRIME_SEVERITY.Heinous || p_crimeData.crimeSeverity == CRIME_SEVERITY.Serious)
			{
				SharedOpinionModifier p_modifier = RelationshipManager.Instance.CreateNewFactionCriminalOpinionModifier(p_crimeData.crimeType, p_crimeData.crimeSeverity, p_character);
				base.owner.opinionComponent.AddOpinionModifier(p_character, p_modifier);
			}
		}
	}

	private void RemoveWantedCharacter(Character p_character)
	{
		if (wantedCharacters.Remove(p_character))
		{
			Messenger.Broadcast(FactionSignals.NO_LONGER_WANTED_CRIMINAL_OF_FACTION, p_character, base.owner);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		RemoveWantedCharacter(p_character);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		wantedCharacters.Contains(p_character);
	}

	public void OnDisbandFaction()
	{
		wantedCharacters?.Clear();
	}
}
