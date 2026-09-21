using System;
using System.Collections;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class BrimstonesTileObject : AOESpellTileObject
{
	private float _timeInterval;

	private float _currentTime;

	public bool isBrimstoneCastedByPlayer { get; private set; }

	public int currentBrimstonesDuration { get; private set; }

	public override Type serializedData => typeof(SaveDataBrimstonesTileObject);

	protected override int effectRadius => 6;

	public BrimstonesTileObject()
		: base(TILE_OBJECT_TYPE.BRIMSTONES_TILE_OBJECT)
	{
		currentBrimstonesDuration = 0;
	}

	public BrimstonesTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataBrimstonesTileObject saveDataBrimstonesTileObject = data as SaveDataBrimstonesTileObject;
		isBrimstoneCastedByPlayer = saveDataBrimstonesTileObject.isBrimstoneCastedByPlayer;
		currentBrimstonesDuration = saveDataBrimstonesTileObject.remainingBrimstoneDuration;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		StartBrimstones();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		StopBrimstones();
		base.OnDestroyPOI(p_destroyer);
	}

	private void StartBrimstones()
	{
		_currentTime = 0f;
		RandomizeTimeInterval();
		GameManager.Instance.StartCoroutine(CommenceFallingBrimstones());
		Messenger.AddListener(Signals.TICK_STARTED, PerTickBrimstones);
	}

	private void StopBrimstones()
	{
		GameManager.Instance.StopCoroutine(CommenceFallingBrimstones());
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickBrimstones);
		AkSoundEngine.StopAll(base.mapObjectVisual.gameObject);
	}

	private IEnumerator CommenceFallingBrimstones()
	{
		while (gridTileLocation != null)
		{
			if (GameManager.Instance.gameHasStarted && !GameManager.Instance.isPaused)
			{
				_currentTime += Time.deltaTime;
				if (_currentTime >= _timeInterval)
				{
					if (affectedTiles == null || affectedTiles.Count <= 0)
					{
						break;
					}
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(affectedTiles);
					GameManager.Instance.CreateParticleEffectAt(randomElement, PARTICLE_EFFECT.Brimstones).GetComponent<BrimstonesParticleEffect>().IsCastedByPlayer = isBrimstoneCastedByPlayer;
					RandomizeTimeInterval();
					_currentTime = 0f;
				}
			}
			yield return null;
		}
	}

	private void RandomizeTimeInterval()
	{
		_timeInterval = UnityEngine.Random.Range(0.1f, 0.7f);
	}

	private void PerTickBrimstones()
	{
		currentBrimstonesDuration++;
		if (isBrimstoneCastedByPlayer)
		{
			if (currentBrimstonesDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BRIMSTONES))
			{
				gridTileLocation.structure.RemovePOI(this);
			}
		}
		else if (currentBrimstonesDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.BRIMSTONES, 0))
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public void ResetDuration()
	{
		currentBrimstonesDuration = 0;
	}

	public override string GetAOESpellTestingData()
	{
		return base.GetAOESpellTestingData() + "\n\tCurrent Duration: " + currentBrimstonesDuration;
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isBrimstoneCastedByPlayer = p_state;
	}
}
