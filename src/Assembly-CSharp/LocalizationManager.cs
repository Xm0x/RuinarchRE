using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;
using UtilityScripts;

public class LocalizationManager : BaseMonoBehaviour
{
	public static LocalizationManager Instance;

	public static string He;

	public static string Him;

	public static string His;

	public static string Himself;

	public static string She;

	public static string Her;

	public static string Herself;

	public static string And;

	public static string The;

	public static string Hers;

	public static string Capitalized_Day;

	public static string Day;

	public static string Days;

	public static string Minute;

	public static string Minutes;

	public static string Hour;

	public static string Hours;

	public static string Object_Used_By;

	public static string Level;

	public static string Time_AM;

	public static string Time_PM;

	public static string Not_Applicable;

	public static Dictionary<string, string> sourceMalePronouns = new Dictionary<string, string>();

	public static Dictionary<string, string> sourceFemalePronouns = new Dictionary<string, string>();

	public static Dictionary<string, string> targetMalePronouns = new Dictionary<string, string>();

	public static Dictionary<string, string> targetFemalePronouns = new Dictionary<string, string>();

	public static Dictionary<string, string> sourcePronouns = new Dictionary<string, string>();

	public static Dictionary<string, string> targetPronouns = new Dictionary<string, string>();

	public bool allowChangeLanguage;

	[Header("Language Specific Fonts")]
	public LocaleFontDictionary localeFontDictionary;

	[Header("For Testing")]
	[SerializeField]
	private string[] updateSingleTableName;

	[SerializeField]
	private string updateTableLocale;

	public LocalizationManagerEventDispatcher eventDispatcher { get; private set; }

	private void Awake()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		if (Instance == null)
		{
			Instance = this;
			eventDispatcher = new LocalizationManagerEventDispatcher();
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
			SceneManager.sceneLoaded += OnSceneLoaded;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Instance == this)
		{
			eventDispatcher?.Cleanup();
			eventDispatcher = null;
			LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			Instance = null;
		}
	}

	private void OnLocaleChanged(Locale obj)
	{
		ReevaluateStaticLocalizedWords();
		PsychopathyData.ConstructLocalizedTexts();
	}

	private void OnSceneLoaded(Scene p_scene1, LoadSceneMode p_mode)
	{
		eventDispatcher.Cleanup();
	}

	internal void Initialize()
	{
		PsychopathyData.ConstructLocalizedTexts();
		ReevaluateStaticLocalizedWords();
		ConstructPronounDictionary();
		ConstructLogParameters();
	}

	private void ReevaluateStaticLocalizedWords()
	{
		He = GetLocalizedValue("Articles_Table", "he");
		Him = GetLocalizedValue("Articles_Table", "him");
		His = GetLocalizedValue("Articles_Table", "his");
		Himself = GetLocalizedValue("Articles_Table", "himself");
		She = GetLocalizedValue("Articles_Table", "she");
		Her = GetLocalizedValue("Articles_Table", "her");
		Herself = GetLocalizedValue("Articles_Table", "herself");
		And = GetLocalizedValue("Articles_Table", "and");
		The = GetLocalizedValue("Articles_Table", "the");
		Hers = GetLocalizedValue("Articles_Table", "hers");
		Capitalized_Day = GetLocalizedValue("UIStrings_Table", "Capitalized_Day");
		Day = GetLocalizedValue("UIStrings_Table", "Day");
		Days = GetLocalizedValue("UIStrings_Table", "Days");
		Minute = GetLocalizedValue("UIStrings_Table", "Minute");
		Minutes = GetLocalizedValue("UIStrings_Table", "Minutes");
		Hour = GetLocalizedValue("UIStrings_Table", "Hour");
		Hours = GetLocalizedValue("Plague_Table", "Hours");
		Object_Used_By = GetLocalizedValue("TileObjects_Table", "Object_Used_By");
		Level = GetLocalizedValue("UIStrings_Table", "Level");
		Time_AM = GetLocalizedValue("UIStrings_Table", "Time_AM");
		Time_PM = GetLocalizedValue("UIStrings_Table", "Time_PM");
		Not_Applicable = GetLocalizedValue("UIStrings_Table", "Not_Applicable");
	}

	private void ConstructPronounDictionary()
	{
		sourceMalePronouns.Clear();
		sourceMalePronouns.Add("sourcePronounSU", Utilities.FirstLetterToUpperCase(He));
		sourceMalePronouns.Add("sourcePronounSL", He);
		sourceMalePronouns.Add("sourcePronounOU", Utilities.FirstLetterToUpperCase(Him));
		sourceMalePronouns.Add("sourcePronounOL", Him);
		sourceMalePronouns.Add("sourcePronounPU", Utilities.FirstLetterToUpperCase(His));
		sourceMalePronouns.Add("sourcePronounPL", His);
		sourceMalePronouns.Add("sourcePronounRU", Utilities.FirstLetterToUpperCase(Himself));
		sourceMalePronouns.Add("sourcePronounRL", Himself);
		sourceMalePronouns.Add("sourcePronounAU", Utilities.FirstLetterToUpperCase(His));
		sourceMalePronouns.Add("sourcePronounAL", His);
		sourceFemalePronouns.Clear();
		sourceFemalePronouns.Add("sourcePronounSU", Utilities.FirstLetterToUpperCase(She));
		sourceFemalePronouns.Add("sourcePronounSL", She);
		sourceFemalePronouns.Add("sourcePronounOU", Utilities.FirstLetterToUpperCase(Her));
		sourceFemalePronouns.Add("sourcePronounOL", Her);
		sourceFemalePronouns.Add("sourcePronounPU", Utilities.FirstLetterToUpperCase(Her));
		sourceFemalePronouns.Add("sourcePronounPL", Her);
		sourceFemalePronouns.Add("sourcePronounRU", Utilities.FirstLetterToUpperCase(Herself));
		sourceFemalePronouns.Add("sourcePronounRL", Herself);
		sourceFemalePronouns.Add("sourcePronounAU", Utilities.FirstLetterToUpperCase(Hers));
		sourceFemalePronouns.Add("sourcePronounAL", Hers);
		targetMalePronouns.Clear();
		targetMalePronouns.Add("targetPronounSU", Utilities.FirstLetterToUpperCase(He));
		targetMalePronouns.Add("targetPronounSL", He);
		targetMalePronouns.Add("targetPronounOU", Utilities.FirstLetterToUpperCase(Him));
		targetMalePronouns.Add("targetPronounOL", Him);
		targetMalePronouns.Add("targetPronounPU", Utilities.FirstLetterToUpperCase(His));
		targetMalePronouns.Add("targetPronounPL", His);
		targetMalePronouns.Add("targetPronounRU", Utilities.FirstLetterToUpperCase(Himself));
		targetMalePronouns.Add("targetPronounRL", Himself);
		targetMalePronouns.Add("targetPronounAU", Utilities.FirstLetterToUpperCase(His));
		targetMalePronouns.Add("targetPronounAL", His);
		targetFemalePronouns.Clear();
		targetFemalePronouns.Add("targetPronounSU", Utilities.FirstLetterToUpperCase(She));
		targetFemalePronouns.Add("targetPronounSL", She);
		targetFemalePronouns.Add("targetPronounOU", Utilities.FirstLetterToUpperCase(Her));
		targetFemalePronouns.Add("targetPronounOL", Her);
		targetFemalePronouns.Add("targetPronounPU", Utilities.FirstLetterToUpperCase(Her));
		targetFemalePronouns.Add("targetPronounPL", Her);
		targetFemalePronouns.Add("targetPronounRU", Utilities.FirstLetterToUpperCase(Herself));
		targetFemalePronouns.Add("targetPronounRL", Herself);
		targetFemalePronouns.Add("targetPronounAU", Utilities.FirstLetterToUpperCase(Hers));
		targetFemalePronouns.Add("targetPronounAL", Hers);
	}

	private void ConstructLogParameters()
	{
		sourcePronouns = sourceMalePronouns;
		targetPronouns = targetMalePronouns;
	}

	public string GetLocalizedValue(string table, string key)
	{
		return GetRawLocalizedValue(table, key);
	}

	public string GetLocalizedValue(string table, string key, Dictionary<string, string> args)
	{
		string text = GetRawLocalizedValue(table, key);
		if (!string.IsNullOrEmpty(text))
		{
			text = LocalizationSettings.StringDatabase.SmartFormatter.Format(text, args);
		}
		return text;
	}

	public bool HasLocalizedValue(string table, string key)
	{
		StringTable table2 = LocalizationSettings.StringDatabase.GetTable(table);
		if (table2 != null)
		{
			TableEntry entry = table2.GetEntry(key);
			if (entry != null)
			{
				return HasLocalizedValue(entry.LocalizedValue);
			}
		}
		return false;
	}

	private string GetRawLocalizedValue(string table, string key)
	{
		StringTable table2 = LocalizationSettings.StringDatabase.GetTable(table);
		if (table2 != null)
		{
			StringTableEntry entry = table2.GetEntry(key);
			if (entry != null)
			{
				return entry.LocalizedValue;
			}
		}
		return string.Empty;
	}

	public bool HasLocalizedValue(string result)
	{
		return !string.IsNullOrEmpty(result);
	}

	public string GetRandomEntry(string p_table)
	{
		StringTable table = LocalizationSettings.StringDatabase.GetTable(p_table);
		int index = Random.Range(0, table.SharedData.Entries.Count);
		SharedTableData.SharedTableEntry sharedTableEntry = table.SharedData.Entries[index];
		return table.GetEntry(sharedTableEntry.Id).Value;
	}

	public bool HasSpecifiedFontForLocale(Locale p_locale)
	{
		return localeFontDictionary.ContainsKey(p_locale.LocaleName);
	}

	public LocalizedFontSettings GetSpecifiedFontSettingsForLocale(Locale p_locale)
	{
		if (localeFontDictionary.ContainsKey(p_locale.LocaleName))
		{
			return localeFontDictionary[p_locale.LocaleName];
		}
		return null;
	}

	public Locale GetLocale(string p_code)
	{
		return LocalizationSettings.Instance.GetAvailableLocales().GetLocale(new LocaleIdentifier(p_code));
	}
}
