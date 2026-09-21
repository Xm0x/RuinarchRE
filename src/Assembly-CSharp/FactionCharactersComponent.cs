using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class FactionCharactersComponent : FactionComponent
{
	public Dictionary<Character, AwarenessData> characterAwarenessData { get; private set; }

	public FactionCharactersComponent()
	{
		characterAwarenessData = new Dictionary<Character, AwarenessData>();
	}

	public FactionCharactersComponent(SaveDataFactionCharactersComponent data)
	{
		characterAwarenessData = new Dictionary<Character, AwarenessData>();
	}

	public void AddListeners(bool shouldLock)
	{
		Messenger.AddListener<Character>(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, UpdateCharacterAwarenessData, shouldLock);
	}

	public void RemoveListeners()
	{
		Messenger.RemoveListener<Character>(CharacterSignals.UPDATE_CHARACTER_AWARENESS_STATE, UpdateCharacterAwarenessData);
	}

	public void UpdateCharacterAwarenessData(Character p_character)
	{
		if (!p_character.isVagrantOrFactionless)
		{
			if (p_character.gridTileLocation == null)
			{
				SetAwarenessState(p_character, AWARENESS_STATE.Missing);
			}
			else if (p_character.traitContainer.HasTrait("Restrained", "Paralyzed", "Stoned"))
			{
				if (p_character.gridTileLocation.IsPartOfSettlement(out var settlement))
				{
					if (settlement.owner != base.owner)
					{
						SetAwarenessState(p_character, AWARENESS_STATE.Missing);
					}
					else
					{
						SetAwarenessState(p_character, AWARENESS_STATE.Available);
					}
				}
				else
				{
					SetAwarenessState(p_character, AWARENESS_STATE.Missing);
				}
			}
			else
			{
				SetAwarenessState(p_character, AWARENESS_STATE.Available);
			}
		}
		else if (characterAwarenessData.ContainsKey(p_character))
		{
			SetAwarenessState(p_character, AWARENESS_STATE.Available);
		}
	}

	private void SetAwarenessState(Character p_character, AWARENESS_STATE p_state)
	{
		if (p_character.hasBeenCleanedUp)
		{
			return;
		}
		if (!characterAwarenessData.ContainsKey(p_character))
		{
			AwarenessData awarenessData = new AwarenessData();
			awarenessData.SetTarget(p_character);
			awarenessData.SetAwarenessState(p_state);
			characterAwarenessData.Add(p_character, awarenessData);
			if (p_state == AWARENESS_STATE.Missing)
			{
				SchedulePresumedDeadFor(p_character, awarenessData);
			}
			return;
		}
		AwarenessData awarenessData2 = characterAwarenessData[p_character];
		if (awarenessData2 == null)
		{
			awarenessData2 = new AwarenessData();
			awarenessData2.SetTarget(p_character);
			characterAwarenessData[p_character] = awarenessData2;
		}
		if (awarenessData2.state != p_state)
		{
			awarenessData2.SetAwarenessState(p_state);
			if (p_state == AWARENESS_STATE.Missing)
			{
				SchedulePresumedDeadFor(p_character, awarenessData2);
			}
			else
			{
				awarenessData2.SetPresumedDeadDate(default(GameDate), string.Empty);
			}
		}
	}

	public AWARENESS_STATE GetAwarenessState(Character p_character)
	{
		if (characterAwarenessData.ContainsKey(p_character))
		{
			return characterAwarenessData[p_character].state;
		}
		return AWARENESS_STATE.Available;
	}

	public AwarenessData GetAwarenessStateData(Character p_character)
	{
		if (characterAwarenessData.ContainsKey(p_character))
		{
			return characterAwarenessData[p_character];
		}
		return null;
	}

	private void SchedulePresumedDeadFor(Character p_character, AwarenessData p_data)
	{
		GameDate gameDate = GameManager.Instance.Today().AddDays(1);
		string p_scheduleKey = SchedulingManager.Instance.AddEntry(gameDate, delegate
		{
			SetPresumedDeadFor(p_character);
		}, p_character);
		p_data.SetPresumedDeadDate(gameDate, p_scheduleKey);
	}

	private void SetPresumedDeadFor(Character p_character)
	{
		if (!p_character.hasBeenCleanedUp)
		{
			AwarenessData awarenessStateData = GetAwarenessStateData(p_character);
			if (awarenessStateData != null && awarenessStateData.currentPresumedDeadDate == GameManager.Instance.Today())
			{
				SetAwarenessState(p_character, AWARENESS_STATE.Presumed_Dead);
				p_character.stateAwarenessComponent.OnCharacterPresumedDeadBy(base.owner);
			}
		}
	}

	public void LoadReferences(SaveDataFactionCharactersComponent data)
	{
		for (int i = 0; i < data.characterAwarenessData.Count; i++)
		{
			AwarenessData awarenessData = data.characterAwarenessData[i];
			Character c = CharacterManager.Instance.GetCharacterByPersistentID(awarenessData.targetPersistentID);
			if (c == null)
			{
				continue;
			}
			characterAwarenessData.Add(c, awarenessData);
			GameDate currentPresumedDeadDate = awarenessData.currentPresumedDeadDate;
			if (currentPresumedDeadDate.hasValue && awarenessData.state == AWARENESS_STATE.Missing)
			{
				SchedulingManager.Instance.AddEntry(currentPresumedDeadDate, delegate
				{
					SetPresumedDeadFor(c);
				}, c);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		characterAwarenessData.ContainsKey(p_character);
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (characterAwarenessData.ContainsKey(p_character))
		{
			AwarenessData awarenessData = characterAwarenessData[p_character];
			if (!string.IsNullOrEmpty(awarenessData.presumedDeadScheduleKey))
			{
				SchedulingManager.Instance.RemoveSpecificEntry(awarenessData.presumedDeadScheduleKey);
			}
			characterAwarenessData.Remove(p_character);
		}
	}

	public void OnDisbandFaction()
	{
		characterAwarenessData?.Clear();
	}
}
