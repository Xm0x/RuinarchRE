using System;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using UnityEngine.Localization;
using UtilityScripts;

[Serializable]
public class Log
{
	public string persistentID;

	public string category;

	public string file;

	public string key;

	public GameDate gameDate;

	public readonly List<LOG_TAG> tags;

	public readonly List<LogFiller> fillers;

	public string actionID;

	public string allInvolvedObjectIDs;

	public string rawText;

	public bool hasBeenFinalized;

	[SerializeField]
	private string _logText;

	[SerializeField]
	private string _unreplacedText;

	public string logText
	{
		get
		{
			if (!hasBeenFinalized)
			{
				FinalizeText();
			}
			return _logText;
		}
	}

	public string unreplacedText => _unreplacedText;

	public bool hasValue
	{
		get
		{
			if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(file))
			{
				return !string.IsNullOrEmpty(key);
			}
			return false;
		}
	}

	public Log()
	{
		fillers = new List<LogFiller>(5);
		tags = new List<LOG_TAG>(5);
		hasBeenFinalized = false;
	}

	public void SetDate(GameDate p_date)
	{
		gameDate = p_date;
	}

	public void SetCategory(string s)
	{
		category = s;
	}

	public void SetFile(string s)
	{
		file = s;
	}

	public void SetKey(string s)
	{
		key = s;
	}

	public void DetermineInitialLogText()
	{
		_logText = GetLocalizedValue();
		_unreplacedText = _logText;
	}

	public void SetConnectedAction(ActualGoapNode node)
	{
		if (node != null)
		{
			actionID = node.persistentID;
		}
	}

	public void SetPersistentID(string p_id)
	{
		persistentID = p_id;
	}

	public void SetInvolvedObjects(string p_involved)
	{
		allInvolvedObjectIDs = p_involved;
	}

	public void SetRawText(string p_text)
	{
		rawText = p_text;
	}

	public void SetFillers(List<LogFiller> p_logFillers)
	{
		fillers.AddRange(p_logFillers);
	}

	public void SetLogText(string p_logText)
	{
		_logText = p_logText;
	}

	public void Copy(Log p_log)
	{
		persistentID = p_log.persistentID;
		category = p_log.category;
		file = p_log.file;
		key = p_log.key;
		gameDate = p_log.gameDate;
		tags.Clear();
		for (int i = 0; i < p_log.tags.Count; i++)
		{
			tags.Add(p_log.tags[i]);
		}
		fillers.Clear();
		AddToFillers(p_log.fillers);
		actionID = p_log.actionID;
		allInvolvedObjectIDs = p_log.allInvolvedObjectIDs;
		rawText = p_log.rawText;
		hasBeenFinalized = p_log.hasBeenFinalized;
		_logText = p_log._logText;
		_unreplacedText = p_log.unreplacedText;
	}

	internal void AddToFillers(ILogFiller obj, string value, LOG_IDENTIFIER identifier, bool replaceExisting = true, bool overrideStringValue = false)
	{
		if (replaceExisting)
		{
			RemoveFillerForIdentifier(identifier);
		}
		SetInvolvedObjects(obj);
		if (overrideStringValue || obj == null)
		{
			fillers.Add(ObjectPoolManager.Instance.CreateNewLogFiller(obj, value, identifier));
		}
		else if (obj != null)
		{
			fillers.Add(ObjectPoolManager.Instance.CreateNewLogFiller(obj, identifier));
		}
	}

	internal void AddToFillers(LogFiller p_filler)
	{
		LogFiller logFiller = ObjectPoolManager.Instance.CreateNewLogFiller();
		logFiller.Copy(p_filler);
		ILogFiller involvedObjects = logFiller.GetObjectForFiller() as ILogFiller;
		SetInvolvedObjects(involvedObjects);
		fillers.Add(logFiller);
	}

	internal void AddToFillers(List<LogFiller> fillers)
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			LogFiller p_filler = fillers[i];
			AddToFillers(p_filler);
		}
	}

	private void SetInvolvedObjects(ILogFiller p_obj)
	{
		if (p_obj == null)
		{
			return;
		}
		AddInvolvedObject(p_obj.persistentID);
		if (p_obj is TileObject tileObject)
		{
			tileObject.OnReferencedInALog();
			if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement is NPCSettlement { locationType: LOCATION_TYPE.VILLAGE } nPCSettlement && !IsInvolved(nPCSettlement))
			{
				AddInvolvedObject(nPCSettlement.persistentID);
			}
		}
		else if (p_obj is Character { homeSettlement: not null } character && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE && !IsInvolved(character.homeSettlement))
		{
			AddInvolvedObject(character.homeSettlement.persistentID);
		}
	}

	private bool HasFillerForIdentifier(LOG_IDENTIFIER identifier)
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			if (fillers[i].identifier == identifier)
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveFillerForIdentifier(LOG_IDENTIFIER identifier)
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			LogFiller logFiller = fillers[i];
			if (logFiller.identifier == identifier)
			{
				ObjectPoolManager.Instance.ReturnLogFillerToPool(logFiller);
				fillers.RemoveAt(i);
			}
		}
	}

	private LogFiller GetFillerForIdentifier(LOG_IDENTIFIER identifier)
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			LogFiller logFiller = fillers[i];
			if (logFiller.identifier == identifier)
			{
				return logFiller;
			}
		}
		return null;
	}

	public bool DoesLogUseIdentifier(LOG_IDENTIFIER logIdentifier)
	{
		if (Utilities.logIdentifierStrings.ContainsKey(logIdentifier))
		{
			string value = Utilities.logIdentifierStrings[logIdentifier];
			if (unreplacedText.Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFillerThatMeetsRequirement(Func<object, bool> requirement)
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			object objectForFiller = fillers[i].GetObjectForFiller();
			if (objectForFiller != null && requirement(objectForFiller))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInvolved(ILogFiller obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(allInvolvedObjectIDs))
		{
			return allInvolvedObjectIDs.Contains(obj.persistentID);
		}
		return false;
	}

	private void AddInvolvedObject(string persistentID)
	{
		allInvolvedObjectIDs = allInvolvedObjectIDs + persistentID + "|";
	}

	public void AddInvolvedObjectManual(string persistentID)
	{
		AddInvolvedObject(persistentID);
		Log self = this;
		DatabaseManager.Instance.mainSQLDatabase.UpdateInvolvedObjects(in self);
	}

	public void ResetText()
	{
		_logText = GetLocalizedValue();
	}

	public void FinalizeText()
	{
		hasBeenFinalized = true;
		_logText = Utilities.LogReplacer(_logText, fillers);
		rawText = Utilities.RemoveRichText(_logText);
	}

	private string GetLocalizedValue()
	{
		if (!string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(key))
		{
			return LocalizationManager.Instance.GetLocalizedValue(file, key);
		}
		return string.Empty;
	}

	public void ReevaluateTextGivenNewLanguage(Locale p_locale)
	{
		ReEvaluateWholeText();
	}

	public void AddLogToDatabase(bool releaseLogAfter = false)
	{
		DatabaseManager.Instance.mainSQLDatabase.InsertLogUsingMultiThread(this);
		if (releaseLogAfter)
		{
			LogPool.Release(this);
		}
	}

	public void AddTag(LOG_TAG tag)
	{
		if (!tags.Contains(tag))
		{
			tags.Add(tag);
		}
	}

	public void AddTag(LOG_TAG[] tags)
	{
		if (tags != null)
		{
			for (int i = 0; i < tags.Length; i++)
			{
				AddTag(tags[i]);
			}
		}
	}

	public void AddTag(List<LOG_TAG> tags)
	{
		if (tags != null)
		{
			for (int i = 0; i < tags.Count; i++)
			{
				AddTag(tags[i]);
			}
		}
	}

	public bool TryUpdateLogAfterRename(Character updatedCharacter, bool force = false)
	{
		if (IsInvolved(updatedCharacter) || force)
		{
			ResetText();
			FinalizeText();
			return true;
		}
		return false;
	}

	public void ReEvaluateWholeText()
	{
		ResetText();
		FinalizeText();
	}

	public bool IsImportant()
	{
		return tags.Contains(LOG_TAG.Major);
	}

	public void Reset()
	{
		for (int i = 0; i < fillers.Count; i++)
		{
			ObjectPoolManager.Instance.ReturnLogFillerToPool(fillers[i]);
		}
		fillers.Clear();
		tags.Clear();
		persistentID = string.Empty;
		category = string.Empty;
		file = string.Empty;
		key = string.Empty;
		gameDate = default(GameDate);
		actionID = string.Empty;
		allInvolvedObjectIDs = string.Empty;
		rawText = string.Empty;
		hasBeenFinalized = false;
		_logText = string.Empty;
		_unreplacedText = string.Empty;
	}
}
