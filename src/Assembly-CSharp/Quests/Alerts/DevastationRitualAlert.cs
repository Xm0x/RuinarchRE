using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using UnityEngine.Localization;

namespace Quests.Alerts;

public class DevastationRitualAlert : GameAlert
{
	public Character actor { get; private set; }

	public bool hasRitualEndedSuccessfully { get; private set; }

	public override Type serializedData => typeof(SaveDataDevastationRitualAlert);

	public DevastationRitualAlert()
		: base(Game_Alert.Devastation_Ritual_Alert)
	{
	}

	public DevastationRitualAlert(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Devastation_Ritual_Alert)
	{
		if (p_data is SaveDataDevastationRitualAlert saveDataDevastationRitualAlert)
		{
			hasRitualEndedSuccessfully = saveDataDevastationRitualAlert.hasRitualEndedSuccessfully;
		}
	}

	public override void LoadReferences(SaveDataGameAlert data)
	{
		if (data is SaveDataDevastationRitualAlert saveDataDevastationRitualAlert && !string.IsNullOrEmpty(saveDataDevastationRitualAlert.actorID))
		{
			actor = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataDevastationRitualAlert.actorID);
		}
		base.LoadReferences(data);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_FINISHED_DEVASTATION_RITUAL, OnCharacterFinishedDevastationRitual);
	}

	public override void SetAsSpawned()
	{
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_FINISHED_DEVASTATION_RITUAL, OnCharacterFinishedDevastationRitual);
	}

	public override void SetAsActive()
	{
		base.SetAsActive();
		UpdateDisplayName();
	}

	public override void OnLocaleChanged(Locale p_newLocale)
	{
		UpdateDisplayName();
	}

	private void OnCharacterFinishedDevastationRitual(Character p_character)
	{
		if (actor == p_character)
		{
			hasRitualEndedSuccessfully = true;
			UpdateDisplayName();
		}
	}

	public void SetDevastationRitualActor(Character p_character)
	{
		actor = p_character;
	}

	private void UpdateDisplayName()
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("source", actor.uiString);
		string text = ((!hasRitualEndedSuccessfully) ? GetLocalizedString("Devastation_Ritual_Alert_started", dictionary) : GetLocalizedString("Devastation_Ritual_Alert_ended", dictionary));
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		_displayName = text;
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	public override void OnSelectBookmark()
	{
		if (hasRitualEndedSuccessfully)
		{
			if (PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) is ThePortal thePortal)
			{
				thePortal.CenterOnStructure();
			}
		}
		else
		{
			actor.CenterOnCharacter();
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		actor = null;
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_FINISHED_DEVASTATION_RITUAL, OnCharacterFinishedDevastationRitual);
	}
}
