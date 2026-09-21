using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class QuicksandMapObjectVisual : MapObjectVisual<TileObject>
{
	[SerializeField]
	private ParticleSystem _quicksandEffect;

	private string _expiryKey;

	private List<Character> _charactersInRange;

	private int _currentTick;

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	private void Awake()
	{
		visionTrigger = base.transform.GetComponentInChildren<TileObjectVisionTrigger>();
		_charactersInRange = new List<Character>();
	}

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		_currentTick = 0;
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		_expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(3)), Expire, this);
		Messenger.AddListener(Signals.TICK_ENDED, PerTick);
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
		OnSpawnQuicksand();
		if (GameManager.Instance.isPaused)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
		else
		{
			_quicksandEffect.Play();
		}
	}

	public override void Reset()
	{
		base.Reset();
		_expiryKey = string.Empty;
		_charactersInRange.Clear();
		_currentTick = 0;
		_quicksandEffect.Clear();
	}

	private void OnGamePaused(bool isPaused)
	{
		if (isPaused)
		{
			_quicksandEffect.Pause();
		}
		else
		{
			_quicksandEffect.Play();
		}
	}

	private void OnSpawnQuicksand()
	{
		PerFifteenMinutes();
	}

	private void PerTick()
	{
		_currentTick++;
		if (_currentTick >= 3)
		{
			_currentTick = 0;
			PerFifteenMinutes();
		}
	}

	private void PerFifteenMinutes()
	{
		for (int i = 0; i < _charactersInRange.Count; i++)
		{
			Character character = _charactersInRange[i];
			character.AdjustHP(-10, ELEMENTAL_TYPE.Earth, triggerDeath: true, null, null, showHPBar: true);
			if (!character.isDead && !character.traitContainer.HasTrait("Disoriented") && Random.Range(0, 100) < 35)
			{
				character.traitContainer.AddTrait(character, "Disoriented");
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character character)
		{
			AddCharacter(character);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character character)
		{
			RemoveCharacter(character);
		}
	}

	private void AddCharacter(Character character)
	{
		if (!_charactersInRange.Contains(character))
		{
			_charactersInRange.Add(character);
		}
	}

	private void RemoveCharacter(Character character)
	{
		_charactersInRange.Remove(character);
	}

	public void Expire()
	{
		_quicksandEffect.Stop();
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		Messenger.RemoveListener(Signals.TICK_ENDED, PerFifteenMinutes);
		Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
		StartCoroutine(DestroyCoroutine());
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor1Second;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_quicksandEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_quicksandEffect.Pause();
	}
}
