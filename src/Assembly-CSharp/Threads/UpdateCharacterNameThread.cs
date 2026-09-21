using System.Collections.Generic;

namespace Threads;

public class UpdateCharacterNameThread : SQLWorkerItem
{
	private Character _character;

	private List<Log> _affectedLogs;

	public void Initialize(Character character)
	{
		_character = character;
		_affectedLogs = DatabaseManager.Instance.mainSQLDatabase.GetFullLogsMentioning(character.persistentID);
		for (int i = 0; i < _affectedLogs.Count; i++)
		{
			_affectedLogs[i].ReEvaluateWholeText();
		}
	}

	public override void DoMultithread()
	{
		base.DoMultithread();
		LogDatabaseUpdateForCharacter(_character);
	}

	public override void FinishMultithread()
	{
		base.FinishMultithread();
		Messenger.Broadcast(UISignals.LOG_MENTIONING_CHARACTER_UPDATED, _character);
	}

	public override void Reset()
	{
		_character = null;
	}

	private void LogDatabaseUpdateForCharacter(Character character)
	{
		for (int i = 0; i < _affectedLogs.Count; i++)
		{
			Log log = _affectedLogs[i];
			DatabaseManager.Instance.mainSQLDatabase.InsertLog(log);
		}
	}
}
