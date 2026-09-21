using System.Diagnostics;
using Inner_Maps.Location_Structures;

public class LogComponent
{
	private string _planCostLog;

	public LogComponent()
	{
	}

	public LogComponent(SaveDataLogComponent data)
	{
	}

	public void RegisterLog(Log addLog, bool releaseAfter = false)
	{
		if (GameManager.Instance.gameHasStarted)
		{
			addLog.AddLogToDatabase(releaseAfter);
		}
	}

	[Conditional("DEBUG_LOG")]
	public void PrintLogIfActive(string log)
	{
	}

	[Conditional("DEBUG_LOG")]
	public void PrintLogErrorIfActive(string log)
	{
	}

	public void ClearCostLog()
	{
		_planCostLog = string.Empty;
	}

	[Conditional("DEBUG_LOG")]
	public void AppendCostLog(string text)
	{
		_planCostLog += text;
	}

	[Conditional("DEBUG_LOG")]
	public void PrintCostLog()
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
