using System.Collections;

public abstract class MapGenerationComponent
{
	public bool succeess = true;

	public string log = "";

	public virtual IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		yield return null;
	}

	public virtual IEnumerator LoadSavedData(MapGenerationData data, SaveDataCurrentProgress saveData)
	{
		yield return null;
	}

	public virtual void LoadSavedData(object state)
	{
	}

	public void AddLog(string str)
	{
		log = log + "\t-" + str + "\n";
	}
}
