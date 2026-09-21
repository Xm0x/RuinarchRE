using System;
using System.Collections;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class IceteroidsTileObject : AOESpellTileObject
{
	private float _timeInterval;

	private float _currentTime;

	public int currentIceteroidsDuration { get; private set; }

	public override Type serializedData => typeof(SaveDataIceteroidsTileObject);

	protected override int effectRadius => 6;

	public IceteroidsTileObject()
		: base(TILE_OBJECT_TYPE.ICETEROIDS_TILE_OBJECT)
	{
		currentIceteroidsDuration = 0;
	}

	public IceteroidsTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataIceteroidsTileObject saveDataIceteroidsTileObject = data as SaveDataIceteroidsTileObject;
		currentIceteroidsDuration = saveDataIceteroidsTileObject.remainingIceteroidsDuration;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		StartIceteroids();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		StopIceteroids();
	}

	private void StartIceteroids()
	{
		GameManager.Instance.StartCoroutine(CommenceFallingIceteroids());
		Messenger.AddListener(Signals.TICK_STARTED, PerTickIceteroids);
	}

	private void StopIceteroids()
	{
		GameManager.Instance.StopCoroutine(CommenceFallingIceteroids());
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickIceteroids);
	}

	private IEnumerator CommenceFallingIceteroids()
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
					GameManager.Instance.CreateParticleEffectAt(randomElement, PARTICLE_EFFECT.Iceteroids);
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

	private void PerTickIceteroids()
	{
		currentIceteroidsDuration++;
		if (currentIceteroidsDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ICETEROIDS))
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public void ResetDuration()
	{
		currentIceteroidsDuration = 0;
	}

	public override string GetAOESpellTestingData()
	{
		return base.GetAOESpellTestingData() + "\n\tCurrent Duration: " + currentIceteroidsDuration;
	}
}
