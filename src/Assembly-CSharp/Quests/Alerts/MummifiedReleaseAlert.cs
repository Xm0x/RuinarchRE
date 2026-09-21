using System;
using System.Collections.Generic;
using Maccima_Games.Util;

namespace Quests.Alerts;

public class MummifiedReleaseAlert : GameAlert
{
	public Character character { get; private set; }

	public override Type serializedData => typeof(SaveDataMummifiedReleaseAlert);

	public MummifiedReleaseAlert()
		: base(Game_Alert.Mummified_Release_Alert)
	{
	}

	public MummifiedReleaseAlert(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Mummified_Release_Alert)
	{
	}

	public override void SetAsSpawned()
	{
	}

	public void SetCharacter(Character p_character)
	{
		character = p_character;
	}

	public override void OnSelectBookmark()
	{
		base.OnSelectBookmark();
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("target", character.uiString);
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("PlayerAlerts_Table", "cursed_mummy", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cursed");
		PlayerUI.Instance.ShowGeneralConfirmation(localizedValue2, localizedValue, "OK", null, null, autoReplaceBodyText: false);
	}

	public override void LoadReferences(SaveDataGameAlert data)
	{
		base.LoadReferences(data);
		if (data is SaveDataMummifiedReleaseAlert saveDataMummifiedReleaseAlert && !string.IsNullOrEmpty(saveDataMummifiedReleaseAlert.characterID))
		{
			character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataMummifiedReleaseAlert.characterID);
		}
	}

	public override void CleanUp()
	{
		base.CleanUp();
		character = null;
	}
}
