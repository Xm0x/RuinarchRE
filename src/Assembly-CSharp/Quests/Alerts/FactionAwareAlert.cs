using System;
using System.Collections.Generic;
using Maccima_Games.Util;
using UnityEngine.Localization;

namespace Quests.Alerts;

public class FactionAwareAlert : GameAlert
{
	public string factionName { get; private set; }

	public override Type serializedData => typeof(SaveDataFactionAwareAlert);

	public FactionAwareAlert()
		: base(Game_Alert.Faction_Aware_Alert)
	{
	}

	public FactionAwareAlert(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Faction_Aware_Alert)
	{
		SaveDataFactionAwareAlert saveDataFactionAwareAlert = p_data as SaveDataFactionAwareAlert;
		factionName = saveDataFactionAwareAlert.factionName;
	}

	public override void SetAsSpawned()
	{
	}

	public void SetFaction(Faction p_faction)
	{
		factionName = p_faction.name;
		UpdateDisplayName(factionName);
	}

	private void UpdateDisplayName(string p_factionName)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("faction1", p_factionName);
		string localizedString = GetLocalizedString("Faction_Aware_Alert_description", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		_displayName = localizedString;
		base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
	}

	public override void OnLocaleChanged(Locale p_newLocale)
	{
		if (!string.IsNullOrEmpty(factionName))
		{
			UpdateDisplayName(factionName);
		}
	}
}
