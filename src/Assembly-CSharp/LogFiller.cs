using System;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using UtilityScripts;

[Serializable]
public class LogFiller
{
	public Type type;

	public string value;

	public string objPersistentID;

	public LOG_IDENTIFIER identifier;

	[SerializeField]
	private string _linkText;

	[SerializeField]
	private string _uiString;

	public string uiString => GetUIString();

	public void Initialize(ILogFiller obj, string value, LOG_IDENTIFIER identifier)
	{
		if (obj != null)
		{
			type = obj.GetType();
			objPersistentID = obj.persistentID;
		}
		else
		{
			type = null;
			objPersistentID = string.Empty;
		}
		this.value = value;
		this.identifier = identifier;
		_linkText = string.Empty;
		_uiString = string.Empty;
		if (obj == null)
		{
			SetDynamicValueUIString();
		}
	}

	public void Initialize(ILogFiller obj, LOG_IDENTIFIER identifier)
	{
		if (obj != null)
		{
			type = obj.GetType();
			objPersistentID = obj.persistentID;
		}
		else
		{
			type = null;
			objPersistentID = string.Empty;
		}
		value = obj.name;
		this.identifier = identifier;
		_linkText = string.Empty;
		_uiString = string.Empty;
	}

	public void Initialize(string formattedLogFillerString, LOG_IDENTIFIER identifier)
	{
		this.identifier = identifier;
		if (identifier == LOG_IDENTIFIER.STRING_1 || identifier == LOG_IDENTIFIER.STRING_2 || identifier == LOG_IDENTIFIER.PARTY_2)
		{
			value = formattedLogFillerString;
			type = null;
			objPersistentID = string.Empty;
		}
		else
		{
			string[] array = formattedLogFillerString.Split('|');
			if (array.Length == 3)
			{
				string typeName = array[0];
				type = Type.GetType(typeName);
				value = array[1];
				objPersistentID = array[2];
			}
			else
			{
				value = array[0];
				type = null;
				objPersistentID = string.Empty;
			}
		}
		_linkText = string.Empty;
	}

	private string GetUIString()
	{
		if (!string.IsNullOrEmpty(_uiString))
		{
			return _uiString;
		}
		object objectForFiller = GetObjectForFiller();
		if (objectForFiller is LocationStructure locationStructure && locationStructure.structureType.IsVillageStructure())
		{
			return value;
		}
		if (objectForFiller is ILogFiller logFiller)
		{
			return logFiller.uiString;
		}
		return value;
	}

	private void SetDynamicValueUIString()
	{
		object objectForFiller = GetObjectForFiller();
		if (objectForFiller != null)
		{
			if (objectForFiller is Wilderness)
			{
				_uiString = value;
			}
			else if (objectForFiller is Character character)
			{
				_uiString = character.visuals.GetCharacterStringIcon() + "<link=" + GetLinkText() + ">" + Utilities.ColorizeName(value, CharacterManager.Instance.GetCharacterNameColorHex(character)) + "</link>";
			}
			else if (objectForFiller is Faction)
			{
				_uiString = "<link=" + GetLinkText() + ">" + Utilities.ColorizeName(value, FactionManager.Instance.GetFactionNameColorHex()) + "</link>";
			}
			else if (objectForFiller is TileObject)
			{
				_uiString = "<link=" + GetLinkText() + ">" + Utilities.ColorizeName(value) + "</link>";
			}
			else
			{
				_uiString = "<link=" + GetLinkText() + ">" + value + "</link>";
			}
		}
		else
		{
			_uiString = value;
		}
	}

	public object GetObjectForFiller()
	{
		if (type != null && !string.IsNullOrEmpty(objPersistentID))
		{
			return DatabaseManager.Instance.GetObjectFromDatabase(type, objPersistentID);
		}
		return null;
	}

	public string GetLinkText()
	{
		if (string.IsNullOrEmpty(_linkText))
		{
			string text = type.ToString();
			if (type.IsSubclassOf(typeof(TileObject)))
			{
				text = "TileObject";
			}
			else if (type.IsSubclassOf(typeof(LocationStructure)))
			{
				text = "Inner_Maps.Location_Structures.LocationStructure";
			}
			_linkText = text + "|" + objPersistentID;
		}
		return _linkText;
	}

	public string GetSQLText()
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		if (type == null)
		{
			return value.Replace("'", "''");
		}
		return string.Format("{0}|{1}|{2}", type, value.Replace("'", "''"), objPersistentID);
	}

	public string GetPrivateUIString()
	{
		return _uiString;
	}

	public string GetPrivateLinkText()
	{
		return _linkText;
	}

	public override string ToString()
	{
		return (type?.ToString() ?? "NoType") + " - " + value + " - " + objPersistentID + " - " + identifier;
	}

	public void Copy(LogFiller p_filler)
	{
		type = p_filler.type;
		value = p_filler.value;
		objPersistentID = p_filler.objPersistentID;
		identifier = p_filler.identifier;
		_linkText = GetPrivateLinkText();
		_uiString = GetPrivateUIString();
	}

	public void Reset()
	{
		type = null;
		value = string.Empty;
		objPersistentID = string.Empty;
		identifier = LOG_IDENTIFIER.NONE;
		_linkText = string.Empty;
		_uiString = string.Empty;
	}
}
