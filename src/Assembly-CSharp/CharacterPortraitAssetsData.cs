using UnityEngine;

[CreateAssetMenu(fileName = "New Character Portrait Assets", menuName = "Scriptable Objects/Character Portrait Assets")]
public class CharacterPortraitAssetsData : ScriptableObject
{
	[SerializeField]
	private Sprite[] _portraits;

	private bool[] _portraitAvailability;

	public Sprite[] portraits => _portraits;

	public bool[] portraitAvailability => _portraitAvailability;

	public Sprite GetPortraitByIndex(int p_index)
	{
		return _portraits[p_index];
	}

	public int GetRandomAvailablePortraitIndex(bool p_tagAsUnavailable = true)
	{
		int randomAvailablePortraitIndexBase = GetRandomAvailablePortraitIndexBase();
		if (p_tagAsUnavailable)
		{
			_portraitAvailability[randomAvailablePortraitIndexBase] = true;
		}
		return randomAvailablePortraitIndexBase;
	}

	public void SetPortraitAvailability(bool[] p_arg)
	{
		_portraitAvailability = p_arg;
	}

	private int GetRandomAvailablePortraitIndexBase()
	{
		if (_portraitAvailability == null)
		{
			_portraitAvailability = new bool[_portraits.Length];
		}
		else if (!HasAvailablePortraitIndex())
		{
			ResetPortraitAvailability();
		}
		int num = Random.Range(0, _portraitAvailability.Length);
		if (_portraitAvailability[num])
		{
			num = GetNearestAvailablePortraitIndex(num);
		}
		return num;
	}

	private bool HasAvailablePortraitIndex()
	{
		for (int i = 0; i < _portraitAvailability.Length; i++)
		{
			if (!_portraitAvailability[i])
			{
				return true;
			}
		}
		return false;
	}

	private void ResetPortraitAvailability()
	{
		for (int i = 0; i < _portraitAvailability.Length; i++)
		{
			_portraitAvailability[i] = false;
		}
	}

	private int GetNearestAvailablePortraitIndex(int p_startingIndex)
	{
		int num = -1;
		if (Random.Range(0, 2) == 0)
		{
			num = GetNearestAvailablePortraitIndexToTheLeft(p_startingIndex);
			if (num == -1)
			{
				num = GetNearestAvailablePortraitIndexToTheRight(p_startingIndex);
			}
		}
		else
		{
			num = GetNearestAvailablePortraitIndexToTheRight(p_startingIndex);
			if (num == -1)
			{
				num = GetNearestAvailablePortraitIndexToTheLeft(p_startingIndex);
			}
		}
		return num;
	}

	private int GetNearestAvailablePortraitIndexToTheLeft(int p_startingIndex)
	{
		for (int num = p_startingIndex; num >= 0; num--)
		{
			if (!_portraitAvailability[num])
			{
				return num;
			}
		}
		return -1;
	}

	private int GetNearestAvailablePortraitIndexToTheRight(int p_startingIndex)
	{
		for (int i = p_startingIndex; i < _portraitAvailability.Length; i++)
		{
			if (!_portraitAvailability[i])
			{
				return i;
			}
		}
		return -1;
	}
}
