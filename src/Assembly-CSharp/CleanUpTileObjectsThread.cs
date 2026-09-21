using System;
using System.Collections.Generic;
using UtilityScripts;

public class CleanUpTileObjectsThread : Multithread
{
	private List<WeakReference> listToBeCleanedUp;

	private Dictionary<string, WeakReference> dictionaryToBeCleanedUp;

	private Dictionary<string, WeakReference> cleanDictionary;

	public bool isProcessing { get; private set; }

	public override void DoMultithread()
	{
		base.DoMultithread();
		RemoveAllDeadReferences();
	}

	public override void FinishMultithread()
	{
		base.FinishMultithread();
		SetListToBeCleanedUp(null);
		SetIsProcessing(p_state: false);
		DatabaseManager.Instance.tileObjectDatabase.DoneProcessCleanUpDestroyedTileObjects(cleanDictionary);
		cleanDictionary = null;
		DatabaseManager.Instance.tileObjectDatabase.AfterDone();
	}

	private void RemoveAllDeadReferences()
	{
		for (int i = 0; i < listToBeCleanedUp.Count; i++)
		{
			if (!listToBeCleanedUp[i].IsAlive)
			{
				listToBeCleanedUp.RemoveAt(i);
				i--;
			}
		}
		if (dictionaryToBeCleanedUp == null)
		{
			return;
		}
		foreach (string key in dictionaryToBeCleanedUp.Keys)
		{
			WeakReference weakReference = dictionaryToBeCleanedUp[key];
			if (weakReference.IsAlive)
			{
				cleanDictionary.Add(key, weakReference);
			}
		}
	}

	public void SetDictionaryToBeCleanedUp(Dictionary<string, WeakReference> p_dictionaryToBeCleanedUp)
	{
		dictionaryToBeCleanedUp = p_dictionaryToBeCleanedUp;
		cleanDictionary = RuinarchCleanUpDictionaryPool.Claim();
	}

	public void SetListToBeCleanedUp(List<WeakReference> p_list)
	{
		listToBeCleanedUp = p_list;
	}

	public void SetIsProcessing(bool p_state)
	{
		isProcessing = p_state;
	}
}
