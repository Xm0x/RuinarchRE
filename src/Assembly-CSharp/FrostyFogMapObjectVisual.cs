using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class FrostyFogMapObjectVisual : MovingMapObjectVisual
{
	private const float Movement_Speed = 0.3f;

	[SerializeField]
	private ParticleSystem _frostyFogEffect;

	[SerializeField]
	private ParticleSystem _snowFlakesEffect;

	[SerializeField]
	private ParticleSystem _waveEffect;

	private string _expiryKey;

	private List<ITraitable> _objsInRange;

	private FrostyFog owner;

	private int _size;

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	protected override void Update()
	{
		base.Update();
		if (base.isSpawned && base.gridTileLocation == null)
		{
			Expire();
		}
	}

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		if (_objsInRange == null)
		{
			_objsInRange = new List<ITraitable>(50);
		}
		else
		{
			_objsInRange.Clear();
		}
		owner = obj as FrostyFog;
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		if (owner.size > 0)
		{
			SetSize(owner.size);
		}
		MoveToRandomDirectionInitial(0.3f);
		_expiryKey = SchedulingManager.Instance.AddEntry(owner.expiryDate, Expire, this);
		Messenger.AddListener(Signals.TICK_ENDED, PerTick);
		base.isSpawned = true;
		if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
	}

	public override void Reset()
	{
		base.Reset();
		_expiryKey = string.Empty;
		_movement?.Kill();
		_movement = null;
		_objsInRange.Clear();
		_frostyFogEffect.Clear();
		_snowFlakesEffect.Clear();
		_waveEffect.Clear();
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_frostyFogEffect.Pause();
			_snowFlakesEffect.Pause();
			_waveEffect.Pause();
		}
		else
		{
			_frostyFogEffect.Play();
			_snowFlakesEffect.Play();
			_waveEffect.Play();
		}
	}

	public override void SetWorldPosition(Vector3 worldPosition)
	{
		base.SetWorldPosition(worldPosition);
		if (GameManager.Instance.gameHasStarted)
		{
			MoveToRandomDirectionInitial(0.3f);
		}
	}

	private void PerTick()
	{
		if (!base.isSpawned)
		{
			return;
		}
		if (owner.isPlayerSource)
		{
			PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE.FROSTY_FOG);
			PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.FROSTY_FOG).resistanceType.GetElement();
		}
		for (int i = 0; i < _objsInRange.Count; i++)
		{
			ITraitable traitable = _objsInRange[i];
			bool flag = traitable.traitContainer.HasTrait("Freezing");
			traitable.traitContainer.AddTrait(traitable, "Freezing", null, bypassElementalChance: false, -1, 0f, ELEMENTAL_TYPE.Ice);
			Freezing traitOrStatus = traitable.traitContainer.GetTraitOrStatus<Freezing>("Freezing");
			if (traitOrStatus != null)
			{
				traitOrStatus.SetIsPlayerSource(owner.isPlayerSource);
				if (!flag && traitable is TreeObject p_tree && ChanceData.RollChance(CHANCE_TYPE.Unicorn_Spawn) && !WorldHasLivingUnicornNotFromPlayerFaction())
				{
					SummonUnicornsFromTree(p_tree);
				}
			}
		}
	}

	public void SetSize(int size)
	{
		_size = size;
		ChangeScaleBySize();
	}

	private void ChangeScaleBySize()
	{
		Vector3 endValue = new Vector3(_size, _size, _size);
		base.transform.DOScale(new Vector3(_size, _size, 1f), 1f);
		_frostyFogEffect.transform.DOScale(endValue, 1f);
		_snowFlakesEffect.transform.DOScale(endValue, 1f);
		_waveEffect.transform.DOScale(endValue, 1f);
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
			{
				AddObject(traitable);
			}
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
			{
				RemoveObject(traitable);
			}
		}
	}

	private void AddObject(ITraitable obj)
	{
		if (!_objsInRange.Contains(obj))
		{
			_objsInRange.Add(obj);
		}
	}

	private void RemoveObject(ITraitable obj)
	{
		_objsInRange.Remove(obj);
	}

	public void Expire()
	{
		_frostyFogEffect.Stop();
		_snowFlakesEffect.Stop();
		_waveEffect.Stop();
		visionTrigger.SetAllCollidersState(state: false);
		base.isSpawned = false;
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
		}
		Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
		owner.Expire();
		StartCoroutine(DestroyCoroutine());
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor5Seconds;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_frostyFogEffect.Play();
		_snowFlakesEffect.Play();
		_waveEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_frostyFogEffect.Pause();
		_snowFlakesEffect.Pause();
		_waveEffect.Pause();
	}

	private bool WorldHasLivingUnicornNotFromPlayerFaction()
	{
		for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
		{
			Character character = CharacterManager.Instance.allCharacters[i];
			if (!character.isDead && character is Unicorn && (character.faction == null || !character.faction.isPlayerFaction))
			{
				return true;
			}
		}
		return false;
	}

	private void SummonUnicornsFromTree(TreeObject p_tree)
	{
		LocationGridTile locationGridTile = p_tree.gridTileLocation;
		if (locationGridTile == null)
		{
			return;
		}
		LocationStructure locationStructure = locationGridTile.structure;
		if (!locationStructure.structureType.IsSpecialStructure())
		{
			locationStructure = null;
		}
		if (locationGridTile.area == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			LocationGridTile locationGridTile2 = locationGridTile.area.GetRandomPassableTile();
			if (locationGridTile2 == null)
			{
				locationGridTile2 = locationGridTile.area.gridTileComponent.GetRandomTile();
			}
			if (locationGridTile2 != null)
			{
				CharacterManager instance = CharacterManager.Instance;
				Faction defaultFactionForMonster = FactionManager.Instance.GetDefaultFactionForMonster(SUMMON_TYPE.Unicorn);
				LocationStructure homeStructure = locationStructure;
				Summon summon = instance.CreateNewSummon(SUMMON_TYPE.Unicorn, defaultFactionForMonster, null, GridMap.Instance.mainRegion, homeStructure, "", bypassIdeologyChecking: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile2);
				if (summon.homeStructure == null)
				{
					summon.SetTerritory(locationGridTile.area);
				}
			}
		}
	}
}
