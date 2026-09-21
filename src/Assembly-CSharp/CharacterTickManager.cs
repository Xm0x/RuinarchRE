using UnityEngine;

public class CharacterTickManager : MonoBehaviour
{
	public static CharacterTickManager Instance;

	private bool _readyToProcessNextTick;

	private float _startTimeOfLastTick;

	private float _timeOfLastBreak;

	private float _maxTickTime;

	private float _previousStepOvertime;

	private int _startIndex;

	private bool _hasTickStarted;

	private int[] _batchSizes = new int[3] { 10, 20, 30 };

	private int _currentBatchSize;

	private int _processedCharacterCount;

	private void Awake()
	{
		Instance = this;
		_maxTickTime = 0.9f / (float)Application.targetFrameRate;
	}

	private void Update()
	{
		if (!_hasTickStarted)
		{
			return;
		}
		int num = _startIndex + _currentBatchSize;
		_processedCharacterCount = 0;
		for (int i = _startIndex; i < num; i++)
		{
			if (i >= CharacterManager.Instance.allCharacters.Count)
			{
				_startIndex = 0;
				_hasTickStarted = false;
				break;
			}
			Character character = CharacterManager.Instance.allCharacters[i];
			if (character.hasBeenCleanedUp)
			{
				continue;
			}
			if (character.hasSubscribedToSignals)
			{
				character.TickStarted();
			}
			if (character.hasBeenCleanedUp)
			{
				continue;
			}
			if (character.hasMarker)
			{
				character.marker.PerTickMovement();
				character.marker.ProcessAllUnprocessedVisionPOIs();
			}
			if (!character.hasBeenCleanedUp)
			{
				if (character.hasSubscribedToSignals)
				{
					character.TickEnded();
				}
				_processedCharacterCount++;
				if (i >= num - 1)
				{
					_startIndex = num;
					break;
				}
			}
		}
	}

	public void StartUp()
	{
		_readyToProcessNextTick = true;
	}

	private void TickActions()
	{
		_readyToProcessNextTick = false;
		_startTimeOfLastTick = Time.realtimeSinceStartup;
		_timeOfLastBreak = Time.realtimeSinceStartup;
		for (int i = _startIndex; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			character.TickStarted();
			character.TickEnded();
			if (IsOutOfTime())
			{
				_startIndex = i + 1;
				if (_startIndex >= CharacterManager.Instance.allCharacters.Count)
				{
					_startIndex = 0;
				}
				break;
			}
		}
		_readyToProcessNextTick = true;
	}

	private bool IsOutOfTime()
	{
		if (_previousStepOvertime + Time.realtimeSinceStartup - _timeOfLastBreak > _maxTickTime)
		{
			_previousStepOvertime = Mathf.Max(0f, Time.realtimeSinceStartup - _timeOfLastBreak - _maxTickTime);
			_timeOfLastBreak = Time.realtimeSinceStartup;
			return true;
		}
		return false;
	}

	public void TickStarted()
	{
		_hasTickStarted = true;
		if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1)
		{
			_currentBatchSize = _batchSizes[0];
		}
		else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2)
		{
			_currentBatchSize = _batchSizes[1];
		}
		else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4)
		{
			_currentBatchSize = _batchSizes[2];
		}
	}

	public void TickEnded()
	{
		_hasTickStarted = false;
	}
}
