using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Settings;
using Threads;
using UnityEngine;
using UtilityScripts;

namespace Databases.SQLDatabase;

public class RuinarchSQLDatabase : IDisposable
{
	private SQLiteConnection _dbConnection;

	private readonly string _bareBonesLogFields = "persistentID, date_tick, date_day, date_month, date_year, logText, category, key, file, involvedObjects, rawText";

	private readonly string _fullLogsFields;

	private readonly string _selectQuery;

	private readonly string _countQuery;

	private readonly LOG_IDENTIFIER[] _allLogIdentifiersExceptNone;

	public List<LOG_TAG> allLogTags { get; private set; }

	public RuinarchSQLDatabase()
	{
		allLogTags = CollectionUtilities.GetEnumValues<LOG_TAG>().ToList();
		for (int i = 0; i < allLogTags.Count; i++)
		{
			LOG_TAG p_type = allLogTags[i];
			_bareBonesLogFields = _bareBonesLogFields + ", " + p_type.ToStringEnum();
		}
		_fullLogsFields = _bareBonesLogFields;
		List<LOG_IDENTIFIER> list = CollectionUtilities.GetEnumValues<LOG_IDENTIFIER>().ToList();
		list.Remove(LOG_IDENTIFIER.NONE);
		_allLogIdentifiersExceptNone = list.ToArray();
		for (int j = 0; j < _allLogIdentifiersExceptNone.Length; j++)
		{
			LOG_IDENTIFIER p_type2 = _allLogIdentifiersExceptNone[j];
			_fullLogsFields = _fullLogsFields + ", " + p_type2.ToStringEnum();
		}
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterNameUpdated);
		Messenger.AddListener(SettingsSignals.LOG_LIMIT_CHANGED, OnLogLimitChanged);
		_selectQuery = "SELECT " + _bareBonesLogFields + " FROM Logs WHERE involvedObjects LIKE ";
		_countQuery = "SELECT COUNT(*) FROM Logs WHERE involvedObjects LIKE ";
	}

	~RuinarchSQLDatabase()
	{
		Dispose(disposing: false);
	}

	private void ReleaseUnmanagedResources()
	{
		CloseConnection();
		_dbConnection?.Dispose();
	}

	private void Dispose(bool disposing)
	{
		ReleaseUnmanagedResources();
		if (disposing)
		{
			Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterNameUpdated);
			Messenger.RemoveListener(SettingsSignals.LOG_LIMIT_CHANGED, OnLogLimitChanged);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void InitializeDatabase()
	{
		OpenConnection();
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		string text = "CREATE TABLE IF NOT EXISTS 'Logs' ('persistentID' STRING PRIMARY KEY UNIQUE, 'category' STRING NOT NULL, 'file' STRING NOT NULL, 'key' STRING NOT NULL, 'date_tick' INTEGER NOT NULL, 'date_day' INTEGER NOT NULL, 'date_month' INTEGER NOT NULL, 'date_year' INTEGER NOT NULL, 'logText' STRING NOT NULL, 'rawText' STRING NOT NULL COLLATE NOCASE, 'actionID' STRING, 'involvedObjects' STRING, 'isIntel' BOOLEAN DEFAULT false, ";
		LOG_IDENTIFIER[] enumValues = CollectionUtilities.GetEnumValues<LOG_IDENTIFIER>();
		foreach (LOG_IDENTIFIER lOG_IDENTIFIER in enumValues)
		{
			if (lOG_IDENTIFIER != LOG_IDENTIFIER.NONE)
			{
				text = text + "'" + lOG_IDENTIFIER.ToStringEnum() + "' STRING, ";
			}
		}
		LOG_TAG[] enumValues2 = CollectionUtilities.GetEnumValues<LOG_TAG>();
		for (int j = 0; j < enumValues2.Length; j++)
		{
			LOG_TAG p_type = enumValues2[j];
			text = text + "'" + p_type.ToStringEnum() + "' BOOLEAN DEFAULT false";
			if (j + 1 < enumValues2.Length)
			{
				text += ", ";
			}
		}
		text += ");";
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = text;
		sQLiteCommand.ExecuteNonQuery();
		List<string> list = new List<string>();
		text = "PRAGMA table_info (Logs)";
		sQLiteCommand.CommandText = text;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		while (dataReader.Read())
		{
			string item = dataReader.GetString(1);
			list.Add(item);
		}
		dataReader.Close();
		foreach (LOG_TAG p_type2 in enumValues2)
		{
			if (!list.Contains(p_type2.ToStringEnum()))
			{
				text = "ALTER TABLE Logs ADD COLUMN " + p_type2.ToStringEnum() + " BOOLEAN DEFAULT false";
				sQLiteCommand.CommandText = text;
				sQLiteCommand.ExecuteNonQuery();
			}
		}
	}

	private void CloseConnection()
	{
		_dbConnection?.Close();
		_dbConnection?.Dispose();
		_dbConnection = null;
	}

	private void OpenConnection()
	{
		_dbConnection = new SQLiteConnection("Data Source=:memory:;");
		_dbConnection.Open();
		if (SaveManager.Instance.useSaveData)
		{
			LoadDatabaseFromFileToMemory(Utilities.gameSavePath + "/Temp/gameDB.db");
		}
	}

	public void InsertLogUsingMultiThread(Log log)
	{
		SQLLogInsertThread sQLLogInsertThread = ObjectPoolManager.Instance.CreateNewSQLInsertThread();
		sQLLogInsertThread.Initialize(log);
		MultiThreadPool.Instance.AddToThreadPool(sQLLogInsertThread);
	}

	public void InsertLogAndDeleteOldest(Log log, out Log deletedLog)
	{
		InsertLog(log);
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandText = "SELECT COUNT(*) FROM 'Logs'";
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		int num = 0;
		while (dataReader.Read())
		{
			num = dataReader.GetInt32(0);
		}
		dataReader.Close();
		deletedLog = null;
		if (num > SettingsManager.Instance.settings.logLimit)
		{
			deletedLog = DeleteOldestLog();
		}
		sQLiteCommand.Dispose();
	}

	public void InsertLog(Log log)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		string text = log.logText.Replace("'", "''");
		string text2 = log.rawText.Replace("'", "''");
		string text3 = "INSERT OR REPLACE INTO 'Logs'('persistentID', 'category', 'file', 'key', 'date_tick', 'date_day', 'date_month', 'date_year', 'logText', 'rawText', 'actionID', 'involvedObjects'";
		if (log.fillers.Count > 0)
		{
			for (int i = 0; i < log.fillers.Count; i++)
			{
				LogFiller logFiller = log.fillers[i];
				text3 = text3 + ", '" + logFiller.identifier.ToStringEnum() + "'";
			}
		}
		if (log.tags.Count > 0)
		{
			for (int j = 0; j < log.tags.Count; j++)
			{
				LOG_TAG p_type = log.tags[j];
				text3 = text3 + ", '" + p_type.ToStringEnum() + "'";
				if (j + 1 == log.tags.Count)
				{
					text3 += ")";
				}
			}
		}
		else
		{
			text3 += ")";
		}
		string text4 = "VALUES ('" + log.persistentID + "', '" + log.category + "', '" + log.file + "', '" + log.key + "', '" + log.gameDate.tick + "', '" + log.gameDate.day + "', '" + log.gameDate.month + "', '" + log.gameDate.year + "', '" + text + "', '" + text2 + "', '" + log.actionID + "', '" + log.allInvolvedObjectIDs + "'";
		if (log.fillers.Count > 0)
		{
			for (int k = 0; k < log.fillers.Count; k++)
			{
				LogFiller logFiller2 = log.fillers[k];
				text4 = text4 + ", '" + logFiller2.GetSQLText() + "'";
			}
		}
		if (log.tags.Count > 0)
		{
			for (int l = 0; l < log.tags.Count; l++)
			{
				text4 += ", '1'";
				if (l + 1 == log.tags.Count)
				{
					text4 += ")";
				}
			}
		}
		else
		{
			text4 += ")";
		}
		string commandText = text3 + " " + text4;
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = commandText;
		sQLiteCommand.ExecuteNonQuery();
	}

	public void PopulateLogsThatMatchCriteria(List<Log> p_logs, string p_persistentID, string p_textLike, List<LOG_TAG> p_tags, int p_startLimitRange = -1, int p_limitRangeLength = 0)
	{
		if (p_tags == null || p_tags.Count == 0)
		{
			return;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		StringBuilder stringBuilder = new StringBuilder(_selectQuery);
		stringBuilder.Append("'%" + p_persistentID + "%'");
		if (!string.IsNullOrEmpty(p_textLike))
		{
			p_textLike = p_textLike.Replace("'", "''");
			stringBuilder.Append(" AND rawText LIKE ");
			stringBuilder.Append("'%" + p_textLike + "%'");
		}
		if (p_tags.Count > 0)
		{
			stringBuilder.Append(" AND (");
			for (int i = 0; i < p_tags.Count; i++)
			{
				LOG_TAG p_type = p_tags[i];
				stringBuilder.Append(p_type.ToStringEnum());
				stringBuilder.Append(" = '1'");
				if (i + 1 < p_tags.Count)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.AppendLine(")");
		}
		stringBuilder.AppendLine("ORDER BY date_year DESC, date_month DESC, date_day DESC, date_tick DESC");
		if (p_limitRangeLength > 0)
		{
			stringBuilder.Append("LIMIT ");
			stringBuilder.Append(p_limitRangeLength);
			if (p_startLimitRange >= 0)
			{
				stringBuilder.Append(" OFFSET ");
				stringBuilder.Append(p_startLimitRange);
			}
		}
		sQLiteCommand.CommandText = stringBuilder.ToString();
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		while (dataReader.Read())
		{
			Log item = ConvertToBareBonesLog(dataReader);
			p_logs.Add(item);
		}
		dataReader.Close();
	}

	public int GetRowCountThatMatchCriteria(List<Log> p_logs, string p_persistentID, string p_textLike, List<LOG_TAG> p_tags)
	{
		int result = 0;
		if (p_tags == null || p_tags.Count == 0)
		{
			return result;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		StringBuilder stringBuilder = new StringBuilder(_countQuery);
		stringBuilder.Append("'%" + p_persistentID + "%'");
		if (!string.IsNullOrEmpty(p_textLike))
		{
			p_textLike = p_textLike.Replace("'", "''");
			stringBuilder.Append(" AND rawText LIKE ");
			stringBuilder.Append("'%" + p_textLike + "%'");
		}
		if (p_tags.Count > 0)
		{
			stringBuilder.Append(" AND (");
			for (int i = 0; i < p_tags.Count; i++)
			{
				LOG_TAG p_type = p_tags[i];
				stringBuilder.Append(p_type.ToStringEnum());
				stringBuilder.Append(" = '1'");
				if (i + 1 < p_tags.Count)
				{
					stringBuilder.Append(" OR ");
				}
			}
			stringBuilder.Append(")");
		}
		sQLiteCommand.CommandText = stringBuilder.ToString();
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		while (dataReader.Read())
		{
			result = dataReader.GetInt32(0);
		}
		dataReader.Close();
		return result;
	}

	public List<Log> GetLogsThatMatchCriteria(string persistentID, string textLike, List<LOG_TAG> tags, int limit = -1)
	{
		if (tags.Count == 0)
		{
			return null;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		string text = "SELECT " + _bareBonesLogFields + " FROM Logs WHERE involvedObjects LIKE '%" + persistentID + "%'";
		if (!string.IsNullOrEmpty(textLike))
		{
			textLike = textLike.Replace("'", "''");
			text = text + " AND rawText LIKE '%" + textLike + "%'";
		}
		if (tags.Count > 0)
		{
			text += " AND (";
			for (int i = 0; i < tags.Count; i++)
			{
				LOG_TAG p_type = tags[i];
				text = text + " " + p_type.ToStringEnum() + " = '1'";
				if (i + 1 < tags.Count)
				{
					text += " OR";
				}
				else if (i + 1 == tags.Count)
				{
					text += ")";
				}
			}
		}
		text += " ORDER BY date_year ASC, date_month ASC, date_day ASC, date_tick ASC";
		if (limit != -1)
		{
			text = text + " LIMIT " + limit;
		}
		sQLiteCommand.CommandText = text;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<Log> list = RuinarchListPool<Log>.Claim();
		while (dataReader.Read())
		{
			Log item = ConvertToBareBonesLog(dataReader);
			list.Add(item);
		}
		dataReader.Close();
		return list;
	}

	public List<Log> GetLogsThatMatchCriteria(List<string> involvedObjects, string textLike, List<LOG_TAG> tags, int limit = -1)
	{
		if (tags.Count == 0)
		{
			return null;
		}
		if (involvedObjects.Count == 0)
		{
			return null;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		string text = "SELECT " + _bareBonesLogFields + " FROM Logs WHERE(";
		for (int i = 0; i < involvedObjects.Count; i++)
		{
			string text2 = involvedObjects[i];
			text = text + "involvedObjects LIKE '%" + text2 + "%'";
			if (i + 1 < involvedObjects.Count)
			{
				text += " OR ";
			}
		}
		text += ")";
		if (!string.IsNullOrEmpty(textLike))
		{
			textLike = textLike.Replace("'", "''");
			text = text + " AND rawText LIKE '%" + textLike + "%'";
		}
		if (tags.Count > 0)
		{
			text += " AND (";
			for (int j = 0; j < tags.Count; j++)
			{
				LOG_TAG p_type = tags[j];
				text = text + " " + p_type.ToStringEnum() + " = '1'";
				if (j + 1 < tags.Count)
				{
					text += " OR";
				}
				else if (j + 1 == tags.Count)
				{
					text += ")";
				}
			}
		}
		text += " ORDER BY date_year DESC, date_month DESC, date_day DESC, date_tick DESC";
		if (limit != -1)
		{
			text = text + " LIMIT " + limit;
		}
		sQLiteCommand.CommandText = text;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<Log> list = RuinarchListPool<Log>.Claim();
		while (dataReader.Read())
		{
			Log item = ConvertToBareBonesLog(dataReader);
			list.Add(item);
		}
		dataReader.Close();
		return list;
	}

	public List<string> GetLogIDsThatMatchCriteria(List<string> pool, List<string> involvedObjects, string textLike, List<LOG_TAG> tags, int limit = -1)
	{
		if (tags.Count == 0)
		{
			return null;
		}
		if (pool.Count == 0)
		{
			return null;
		}
		if (involvedObjects.Count == 0)
		{
			return null;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		string text = "SELECT persistentID FROM Logs WHERE(";
		for (int i = 0; i < pool.Count; i++)
		{
			string text2 = pool[i];
			text = text + "persistentID = '" + text2 + "'";
			if (i + 1 < pool.Count)
			{
				text += " OR ";
			}
		}
		text += ")";
		if (involvedObjects.Count > 0)
		{
			text += " AND(";
			for (int j = 0; j < involvedObjects.Count; j++)
			{
				string text3 = involvedObjects[j];
				text = text + "involvedObjects LIKE '%" + text3 + "%'";
				if (j + 1 < involvedObjects.Count)
				{
					text += " OR ";
				}
			}
			text += ")";
		}
		if (!string.IsNullOrEmpty(textLike))
		{
			textLike = textLike.Replace("'", "''");
			text = text + " AND rawText LIKE '%" + textLike + "%'";
		}
		if (tags.Count > 0)
		{
			text += " AND (";
			for (int k = 0; k < tags.Count; k++)
			{
				LOG_TAG p_type = tags[k];
				text = text + " " + p_type.ToStringEnum() + " = '1'";
				if (k + 1 < tags.Count)
				{
					text += " OR";
				}
				else if (k + 1 == tags.Count)
				{
					text += ")";
				}
			}
		}
		text += " ORDER BY date_year DESC, date_month DESC, date_day DESC, date_tick DESC";
		if (limit != -1)
		{
			text = text + " LIMIT " + limit;
		}
		sQLiteCommand.CommandText = text;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<string> list = new List<string>();
		while (dataReader.Read())
		{
			string item = dataReader.GetString(0);
			list.Add(item);
		}
		dataReader.Close();
		return list;
	}

	public List<string> GetLogIDsThatMatchCriteria(List<string> pool, string textLike, List<LOG_TAG> tags, int limit = -1)
	{
		if (tags.Count == 0)
		{
			return null;
		}
		if (pool.Count == 0)
		{
			return null;
		}
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		string text = "SELECT persistentID FROM Logs WHERE(";
		for (int i = 0; i < pool.Count; i++)
		{
			string text2 = pool[i];
			text = text + "persistentID = '" + text2 + "'";
			if (i + 1 < pool.Count)
			{
				text += " OR ";
			}
		}
		text += ")";
		if (!string.IsNullOrEmpty(textLike))
		{
			textLike = textLike.Replace("'", "''");
			text = text + " AND rawText LIKE '%" + textLike + "%'";
		}
		if (tags.Count > 0)
		{
			text += " AND (";
			for (int j = 0; j < tags.Count; j++)
			{
				LOG_TAG p_type = tags[j];
				text = text + " " + p_type.ToStringEnum() + " = '1'";
				if (j + 1 < tags.Count)
				{
					text += " OR";
				}
				else if (j + 1 == tags.Count)
				{
					text += ")";
				}
			}
		}
		text += " ORDER BY date_year DESC, date_month DESC, date_day DESC, date_tick DESC";
		if (limit != -1)
		{
			text = text + " LIMIT " + limit;
		}
		sQLiteCommand.CommandText = text;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<string> list = new List<string>();
		while (dataReader.Read())
		{
			string item = dataReader.GetString(0);
			list.Add(item);
		}
		dataReader.Close();
		return list;
	}

	public Log GetLogWithPersistentID(string persistentID)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "SELECT " + _bareBonesLogFields + " FROM Logs WHERE persistentID = '" + persistentID + "'";
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		if (dataReader.Read())
		{
			return ConvertToBareBonesLog(dataReader);
		}
		dataReader.Close();
		return null;
	}

	public Log GetFullLogWithPersistentID(string persistentID)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "SELECT " + _fullLogsFields + " FROM Logs WHERE persistentID = '" + persistentID + "'";
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		if (dataReader.Read())
		{
			return ConvertToFullLog(dataReader);
		}
		dataReader.Close();
		return null;
	}

	public void SetLogIntelState(string persistentID, bool isIntel)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "UPDATE 'Logs' SET isIntel = " + isIntel + " WHERE persistentID = '" + persistentID + "'";
		sQLiteCommand.ExecuteNonQuery();
		sQLiteCommand.Dispose();
	}

	public void UpdateInvolvedObjects(in Log log)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "UPDATE 'Logs' SET involvedObjects = '" + log.allInvolvedObjectIDs + "' WHERE persistentID = '" + log.persistentID + "'";
		sQLiteCommand.ExecuteNonQuery();
		sQLiteCommand.Dispose();
		Messenger.Broadcast(UISignals.LOG_IN_DATABASE_UPDATED, log);
	}

	private Log DeleteOldestLog()
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "SELECT persistentID FROM Logs ORDER BY rowid LIMIT 1";
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		string text = string.Empty;
		while (dataReader.Read())
		{
			text = dataReader.GetString(0);
		}
		dataReader.Close();
		sQLiteCommand.Dispose();
		if (!string.IsNullOrEmpty(text))
		{
			return DeleteLog(text);
		}
		return null;
	}

	private void DeleteOldestLogs(int p_count)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "SELECT persistentID FROM Logs ORDER BY rowid LIMIT " + p_count;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<string> list = RuinarchListPool<string>.Claim();
		while (dataReader.Read())
		{
			list.Add(dataReader.GetString(0));
		}
		dataReader.Close();
		sQLiteCommand.Dispose();
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			if (!string.IsNullOrEmpty(text))
			{
				Log arg = DeleteLog(text);
				Messenger.Broadcast(UISignals.LOG_REMOVED_FROM_DATABASE, arg);
			}
		}
	}

	private Log DeleteLog(string persistentID)
	{
		Log logWithPersistentID = GetLogWithPersistentID(persistentID);
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		sQLiteCommand.CommandText = "DELETE FROM Logs WHERE persistentID = '" + persistentID + "'";
		sQLiteCommand.ExecuteNonQuery();
		sQLiteCommand.Dispose();
		return logWithPersistentID;
	}

	public List<Log> GetFullLogsMentioning(string persistentID)
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandType = CommandType.Text;
		string commandText = "SELECT " + _fullLogsFields + " FROM Logs WHERE involvedObjects LIKE '%" + persistentID + "%'";
		sQLiteCommand.CommandText = commandText;
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		List<Log> list = RuinarchListPool<Log>.Claim();
		while (dataReader.Read())
		{
			Log item = ConvertToFullLog(dataReader);
			list.Add(item);
		}
		return list;
	}

	private void OnCharacterNameUpdated(Character character)
	{
		UpdateCharacterNameThread updateCharacterNameThread = ObjectPoolManager.Instance.CreateNewLogDatabaseThread();
		updateCharacterNameThread.Initialize(character);
		MultiThreadPool.Instance.AddToThreadPool(updateCharacterNameThread);
	}

	private void OnLogLimitChanged()
	{
		SQLiteCommand sQLiteCommand = _dbConnection.CreateCommand();
		sQLiteCommand.CommandText = "SELECT COUNT(*) FROM 'Logs'";
		IDataReader dataReader = sQLiteCommand.ExecuteReader();
		int num = 0;
		while (dataReader.Read())
		{
			num = dataReader.GetInt32(0);
		}
		dataReader.Close();
		if (num > SettingsManager.Instance.settings.logLimit)
		{
			int p_count = num - SettingsManager.Instance.settings.logLimit;
			DeleteOldestLogs(p_count);
		}
	}

	private Log ConvertToBareBonesLog(IDataReader dataReader)
	{
		string id = dataReader.GetString(0);
		int @int = dataReader.GetInt32(1);
		int int2 = dataReader.GetInt32(2);
		int int3 = dataReader.GetInt32(3);
		int int4 = dataReader.GetInt32(4);
		string logText = dataReader.GetString(5);
		string category = dataReader.GetString(6);
		string key = dataReader.GetString(7);
		string file = dataReader.GetString(8);
		string involvedObjects = dataReader.GetString(9);
		string rawText = dataReader.GetString(10);
		int num = 11;
		List<LOG_TAG> list = new List<LOG_TAG>();
		for (int i = 0; i < allLogTags.Count; i++)
		{
			LOG_TAG item = allLogTags[i];
			if (dataReader.GetBoolean(num))
			{
				list.Add(item);
			}
			num++;
		}
		return GameManager.CreateNewLog(id, new GameDate(int3, int2, int4, @int), logText, category, key, file, involvedObjects, list, rawText);
	}

	private Log ConvertToFullLog(IDataReader dataReader)
	{
		string id = dataReader.GetString(0);
		int @int = dataReader.GetInt32(1);
		int int2 = dataReader.GetInt32(2);
		int int3 = dataReader.GetInt32(3);
		int int4 = dataReader.GetInt32(4);
		string logText = dataReader.GetString(5);
		string category = dataReader.GetString(6);
		string key = dataReader.GetString(7);
		string file = dataReader.GetString(8);
		string involvedObjects = dataReader.GetString(9);
		string rawText = dataReader.GetString(10);
		int num = 11;
		List<LOG_TAG> list = RuinarchListPool<LOG_TAG>.Claim();
		for (int i = 0; i < allLogTags.Count; i++)
		{
			LOG_TAG item = allLogTags[i];
			if (dataReader.GetBoolean(num))
			{
				list.Add(item);
			}
			num++;
		}
		List<LogFiller> list2 = RuinarchListPool<LogFiller>.Claim();
		for (int j = 0; j < _allLogIdentifiersExceptNone.Length; j++)
		{
			LOG_IDENTIFIER p_identifier = _allLogIdentifiersExceptNone[j];
			if (!dataReader.IsDBNull(num))
			{
				object value = dataReader.GetValue(num);
				string text = string.Empty;
				if (value is int num2)
				{
					text = num2.ToString();
				}
				else if (value is string text2)
				{
					text = text2;
				}
				if (!string.IsNullOrEmpty(text))
				{
					LogFiller item2 = ObjectPoolManager.Instance.CreateNewLogFiller(text, p_identifier);
					list2.Add(item2);
				}
			}
			num++;
		}
		Log log = GameManager.CreateNewLog(id, new GameDate(int3, int2, int4, @int), logText, category, key, file, involvedObjects, list, rawText);
		log.SetFillers(list2);
		RuinarchListPool<LOG_TAG>.Release(list);
		RuinarchListPool<LogFiller>.Release(list2);
		return log;
	}

	public void SaveInMemoryDatabaseToFile(string filePath)
	{
		using SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + filePath + ";Version=3;");
		sQLiteConnection.Open();
		_dbConnection.BackupDatabase(sQLiteConnection, "main", "main", -1, null, -1);
		sQLiteConnection.Close();
	}

	public void LoadDatabaseFromFileToMemory(string filePath)
	{
		using SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + filePath + ";Version=3;");
		sQLiteConnection.Open();
		sQLiteConnection.BackupDatabase(_dbConnection, "main", "main", -1, null, -1);
		sQLiteConnection.Close();
	}

	static RuinarchSQLDatabase()
	{
		string environmentVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
		string text = Application.dataPath + "/Plugins";
		if (environmentVariable != null && !environmentVariable.Contains(text))
		{
			Environment.SetEnvironmentVariable("PATH", environmentVariable + "/" + text, EnvironmentVariableTarget.Process);
		}
	}
}
