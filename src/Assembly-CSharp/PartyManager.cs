using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UtilityScripts;

public class PartyManager : MonoBehaviour
{
	public static PartyManager Instance;

	public const int MAX_MEMBER_CAPACITY = 5;

	public PartyNameLayouts partyNameLayouts;

	public PartyNameNouns partyNameNouns;

	public PartyNameAdjectives partyNameAdjectives;

	public PartyNameDeclarations partyNameDeclarations;

	public PartyNameTargets partyNameTargets;

	[Space]
	public PartyNameLayouts partyNameLayoutsPlayerParty;

	public PartyNameNouns partyNameNounsForPlayerParty;

	private List<string> _nounsOriginalPool;

	private List<string> _nounsPlayerPartyPool;

	private static readonly Regex sWhitespace = new Regex("\\s+");

	private void Awake()
	{
		Instance = this;
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
	}

	private void Start()
	{
		LoadLocalizedValuesForPartyNames();
		_nounsOriginalPool = new List<string>(partyNameNouns.nouns);
		_nounsPlayerPartyPool = new List<string>(partyNameNounsForPlayerParty.nouns);
	}

	private void OnDestroy()
	{
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		_nounsOriginalPool.Clear();
		_nounsPlayerPartyPool.Clear();
		_nounsOriginalPool = null;
		_nounsPlayerPartyPool = null;
	}

	public Party CreateNewParty(Character partyCreator, PARTY_QUEST_TYPE p_plannedPartyQuestType = PARTY_QUEST_TYPE.None)
	{
		Party party = ObjectPoolManager.Instance.CreateNewParty();
		party.SetPlannedPartyQuestType(p_plannedPartyQuestType);
		party.Initialize(partyCreator);
		Messenger.Broadcast(PartySignals.PARTY_CREATED, party);
		return party;
	}

	public Party CreatePersistentDemonicDefendParty(Character partyCreator)
	{
		Party party = ObjectPoolManager.Instance.CreateNewParty();
		party.SetPlannedPartyQuestType(PARTY_QUEST_TYPE.None);
		party.Initialize(partyCreator);
		PlayerManager.Instance.player.underlingsComponent.SetPersistentDefendParty(party);
		Messenger.Broadcast(PartySignals.PARTY_CREATED, party);
		return party;
	}

	public Party CreateNewParty(SaveDataParty data)
	{
		Party party = ObjectPoolManager.Instance.CreateNewParty();
		party.Initialize(data);
		return party;
	}

	public PartyQuest CreateNewPartyQuest(PARTY_QUEST_TYPE type)
	{
		string text = type.ToStringEnumNoSpace() + "PartyQuest, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		return (Activator.CreateInstance(Type.GetType(text) ?? throw new Exception("provided party quest type was invalid! " + text)) as PartyQuest) ?? throw new Exception("provided type not a party quest! " + text);
	}

	private SaveDataPartyQuest CreateNewSaveDataPartyQuest(PartyQuest party)
	{
		SaveDataPartyQuest obj = Activator.CreateInstance(party.serializedData) as SaveDataPartyQuest;
		obj.Save(party);
		return obj;
	}

	public PartyQuest CreateNewPartyQuest(SaveDataPartyQuest data)
	{
		return Activator.CreateInstance(Type.GetType(data.partyQuestType.ToStringEnumNoSpace() + "PartyQuest, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"), data) as PartyQuest;
	}

	public string GetNewPartyNameForPlayerParty(Character partyCreator, PARTY_QUEST_TYPE p_type)
	{
		string text;
		switch (p_type)
		{
		case PARTY_QUEST_TYPE.Demon_Raid:
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("DemonicPartyNameLayouts_Table", "Demonic_Raid_Party");
			text = PartyNameLayoutReplacer(localizedValue, isPlayerParty: true, partyCreator);
			break;
		}
		case PARTY_QUEST_TYPE.Demon_Snatch:
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("DemonicPartyNameLayouts_Table", "Demonic_Snatch_Party");
			text = PartyNameLayoutReplacer(localizedValue, isPlayerParty: true, partyCreator);
			break;
		}
		case PARTY_QUEST_TYPE.Demon_Defend:
		case PARTY_QUEST_TYPE.Demon_Rescue:
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("DemonicPartyNameLayouts_Table", "Demonic_Defend_Party");
			text = PartyNameLayoutReplacer(localizedValue, isPlayerParty: true, partyCreator);
			break;
		}
		default:
		{
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("DemonicPartyNameLayouts_Table", "Demonic_Defend_Party");
			text = PartyNameLayoutReplacer(localizedValue, isPlayerParty: true, partyCreator);
			break;
		}
		}
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja" || LocalizationSettings.SelectedLocale.Identifier.Code == "zh")
		{
			ReplaceWhitespace(text, "");
		}
		return text;
	}

	public string GetNewPartyName(Character partyCreator)
	{
		string randomElement = CollectionUtilities.GetRandomElement(partyNameLayouts.layouts);
		string text = PartyNameLayoutReplacer(randomElement, isPlayerParty: false, partyCreator);
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja" || LocalizationSettings.SelectedLocale.Identifier.Code == "zh")
		{
			ReplaceWhitespace(text, "");
		}
		return text;
	}

	public string PartyNameLayoutReplacer(string layout, bool isPlayerParty, Character partyCreator = null)
	{
		List<string> list = RuinarchListPool<string>.Claim();
		Utilities.PopulateSplittedStringAndKeepDelimiters(list, layout, Utilities.delimiters);
		for (int i = 0; i < list.Count; i++)
		{
			string value = string.Empty;
			string text = list[i];
			if (text.StartsWith("[") && text.EndsWith("]"))
			{
				value = ((!isPlayerParty) ? PartyNameFillerReplacer(text, partyCreator) : PartyNameFillerReplacerForPlayerParty(text));
			}
			if (!string.IsNullOrEmpty(value))
			{
				list[i] = value;
			}
		}
		string text2 = string.Empty;
		for (int j = 0; j < list.Count; j++)
		{
			text2 += list[j];
		}
		text2 = text2.Trim(' ');
		RuinarchListPool<string>.Release(list);
		return text2;
	}

	private string PartyNameFillerReplacer(string filler, Character partyCreator)
	{
		string result = filler;
		List<string> list = null;
		switch (filler)
		{
		case "[Noun]":
			if (partyNameNouns.nouns.Count <= 0)
			{
				partyNameNouns.nouns.AddRange(_nounsOriginalPool);
			}
			list = partyNameNouns.nouns;
			break;
		case "[Adjective]":
			list = partyNameAdjectives.adjectives;
			break;
		case "[Target]":
			list = partyNameTargets.targets;
			break;
		case "[Declaration]":
			list = partyNameDeclarations.declarations;
			break;
		case "[Region]":
			if (partyCreator.currentRegion != null)
			{
				result = partyCreator.currentRegion.name;
			}
			break;
		}
		if (list != null)
		{
			int randomIndexInList = CollectionUtilities.GetRandomIndexInList(list);
			result = list[randomIndexInList];
			if (filler == "[Noun]")
			{
				list.RemoveAt(randomIndexInList);
			}
		}
		return result;
	}

	private string PartyNameFillerReplacerForPlayerParty(string filler)
	{
		string result = filler;
		List<string> list = null;
		if (filler == "[Noun]")
		{
			if (partyNameNounsForPlayerParty.nouns.Count <= 0)
			{
				partyNameNounsForPlayerParty.nouns.AddRange(_nounsPlayerPartyPool);
			}
			list = partyNameNounsForPlayerParty.nouns;
		}
		if (list != null)
		{
			int randomIndexInList = CollectionUtilities.GetRandomIndexInList(list);
			result = list[randomIndexInList];
			if (filler == "[Noun]")
			{
				list.RemoveAt(randomIndexInList);
			}
		}
		return result;
	}

	private void OnLocaleChanged(Locale obj)
	{
		LoadLocalizedValuesForPartyNames();
		_nounsOriginalPool = new List<string>(partyNameNouns.nouns);
		_nounsPlayerPartyPool = new List<string>(partyNameNounsForPlayerParty.nouns);
	}

	private void LoadLocalizedValuesForPartyNames()
	{
		LoadLocalizedValuesForList("PartyNameLayouts_Table", partyNameLayouts.layouts);
		LoadLocalizedValuesForList("PartyNameAdjectives_Table", partyNameAdjectives.adjectives);
		LoadLocalizedValuesForList("PartyNameDeclarations_Table", partyNameDeclarations.declarations);
		LoadLocalizedValuesForList("PartyNameNouns_Table", partyNameNouns.nouns);
		LoadLocalizedValuesForList("PartyNameTargets_Table", partyNameTargets.targets);
		LoadLocalizedValuesForList("DemonicPartyNameNouns_Table", partyNameNounsForPlayerParty.nouns);
	}

	private void LoadLocalizedValuesForList(string p_table, List<string> p_list)
	{
		p_list.Clear();
		StringTable table = LocalizationSettings.StringDatabase.GetTable(p_table);
		for (int i = 0; i < table.SharedData.Entries.Count; i++)
		{
			long id = table.SharedData.Entries[i].Id;
			string value = table.GetEntry(id).Value;
			p_list.Add(value);
		}
	}

	public static string ReplaceWhitespace(string input, string replacement)
	{
		return sWhitespace.Replace(input, replacement);
	}
}
