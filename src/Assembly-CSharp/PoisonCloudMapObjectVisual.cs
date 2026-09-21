using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine;
using UtilityScripts;

public class PoisonCloudMapObjectVisual : MovingMapObjectVisual
{
	private const float Movement_Speed = 0.3f;

	[SerializeField]
	private ParticleSystem _cloudEffect;

	[SerializeField]
	private ParticleSystem _explosionEffect;

	[SerializeField]
	private int _size;

	private string _expiryKey;

	private List<TileObject> _objsInRange;

	private List<Character> _charactersInRange;

	private PoisonCloud _poisonCloud;

	private uint _sfxID;

	private Vector3 previousMousePos;

	public bool wasJustPlaced { get; private set; }

	public override void UpdateTileObjectVisual(TileObject obj)
	{
	}

	protected override void Update()
	{
		base.Update();
		if (!base.isSpawned)
		{
			return;
		}
		if (wasJustPlaced)
		{
			wasJustPlaced = false;
		}
		if (base.gridTileLocation == null)
		{
			Expire();
		}
		else
		{
			if (_poisonCloud != PlayerManager.Instance.player.playerSkillComponent.latestCastMovingTileObject || GameManager.Instance.isPaused)
			{
				return;
			}
			Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
			float num = Vector2.Distance(vector, base.worldPos);
			bool flag = _poisonCloud.SetMovementType((num <= 10f) ? Moving_Object_Movement_Type.Follow_Cursor : Moving_Object_Movement_Type.Default);
			switch (_poisonCloud.currentMovementType)
			{
			case Moving_Object_Movement_Type.Default:
				if (flag)
				{
					MoveToRandomDirection(0.3f);
				}
				break;
			case Moving_Object_Movement_Type.Follow_Cursor:
				if (vector != previousMousePos)
				{
					MoveToCursor(vector, 0.3f);
					previousMousePos = vector;
				}
				break;
			}
		}
	}

	public override void Initialize(TileObject obj)
	{
		base.Initialize(obj);
		if (_objsInRange == null)
		{
			_objsInRange = new List<TileObject>(50);
		}
		else
		{
			_objsInRange.Clear();
		}
		if (_charactersInRange == null)
		{
			_charactersInRange = new List<Character>(50);
		}
		else
		{
			_charactersInRange.Clear();
		}
		_poisonCloud = obj as PoisonCloud;
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Poison_Cloud", base.gameObject);
	}

	public override void PlaceObjectAt(LocationGridTile tile)
	{
		base.PlaceObjectAt(tile);
		wasJustPlaced = true;
		if (_poisonCloud.size > 0)
		{
			SetSize(_poisonCloud.size);
		}
		_cloudEffect.gameObject.SetActive(value: true);
		MoveToRandomDirectionInitial(0.3f);
		_expiryKey = SchedulingManager.Instance.AddEntry(_poisonCloud.expiryDate, Expire, this);
		Messenger.AddListener(Signals.TICK_ENDED, PerTick);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		base.isSpawned = true;
		if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
	}

	public override void Reset()
	{
		base.Reset();
		previousMousePos = Vector3.zero;
		_expiryKey = string.Empty;
		_movement?.Kill();
		_movement = null;
		_objsInRange.Clear();
		_charactersInRange.Clear();
		_cloudEffect.Clear();
		_explosionEffect.Clear();
		AkSoundEngine.StopPlayingID(_sfxID);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_cloudEffect.Pause();
			_explosionEffect.Pause();
		}
		else
		{
			_cloudEffect.Play();
			_explosionEffect.Play();
		}
	}

	public void OnNoLongerLatestCastMovingObject()
	{
		MoveToRandomDirection(0.3f);
	}

	private void PerTick()
	{
		if (!base.isSpawned || (_charactersInRange.Count <= 0 && _objsInRange.Count <= 0))
		{
			return;
		}
		float num = 0f;
		ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Poison;
		SkillData source = null;
		if (_poisonCloud.isPlayerSource)
		{
			num = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE.POISON_CLOUD);
			elementalType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.POISON_CLOUD).resistanceType.GetElement();
			source = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.POISON_CLOUD);
		}
		for (int i = 0; i < _charactersInRange.Count; i++)
		{
			Character character = _charactersInRange[i];
			if (character.hasMarker && !character.isDead)
			{
				character.AdjustHP(-20, ELEMENTAL_TYPE.Poison, triggerDeath: true, source, null, showHPBar: true, num, _poisonCloud.isPlayerSource);
			}
		}
		TileObject randomElement = CollectionUtilities.GetRandomElement(_objsInRange);
		if (randomElement != null)
		{
			randomElement.traitContainer.AddTrait(randomElement, "Poisoned", null, bypassElementalChance: false, -1, num, elementalType);
			randomElement.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned")?.SetIsPlayerSource(_poisonCloud.isPlayerSource);
		}
	}

	public void Explode()
	{
		_cloudEffect.TriggerSubEmitter(0);
		_poisonCloud.SetDoExpireEffect(state: false);
		Expire();
		float piercing = 0f;
		int p_damage = 250;
		if (_poisonCloud.isPlayerSource)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.POISON_CLOUD);
			piercing = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(PLAYER_SKILL_TYPE.POISON_CLOUD);
			if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Explosion_Fire_Damage_Level_3))
			{
				p_damage = 550;
			}
			else if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Explosion_Fire_Damage_Level_2))
			{
				p_damage = 450;
			}
			else if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Explosion_Fire_Damage_Level_1))
			{
				p_damage = 350;
			}
			else if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Explosion_Fire_Damage_Level_0))
			{
				p_damage = 250;
			}
		}
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		base.gridTileLocation.PopulateTilesInRadius(list2, _size, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		for (int j = 0; j < list.Count; j++)
		{
			ITraitable traitable = list[j];
			ApplyExplosionEffect(traitable, p_damage, piercing);
		}
		RuinarchListPool<ITraitable>.Release(list);
	}

	private void ApplyExplosionEffect(ITraitable traitable, int p_damage, float piercing)
	{
		traitable.AdjustHP(-p_damage, ELEMENTAL_TYPE.Fire, triggerDeath: true, null, null, showHPBar: true, piercing, _poisonCloud.isPlayerSource);
		if (_poisonCloud.isPlayerSource && traitable is Character character)
		{
			character.OnCharacterHitByPlayerSpell(-p_damage);
		}
	}

	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait)
	{
		if (traitable is TileObject item)
		{
			if (trait is Burning && _objsInRange.Contains(item))
			{
				Explode();
			}
		}
		else if (traitable is Character item2 && trait is Burning && _charactersInRange.Contains(item2))
		{
			Explode();
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (!base.isSpawned)
		{
			return;
		}
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (!(andAddPOIVisionTriggerFromCache != null))
		{
			return;
		}
		if (andAddPOIVisionTriggerFromCache.damageable is PoisonCloud poisonCloud && poisonCloud != _poisonCloud)
		{
			if (!wasJustPlaced)
			{
				CollidedWithPoisonCloud(poisonCloud);
			}
		}
		else if (andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
		{
			AddTraitable(traitable);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable traitable)
			{
				RemoveTraitable(traitable);
			}
		}
	}

	private void CollidedWithPoisonCloud(PoisonCloud otherPoisonCloud)
	{
		if (!otherPoisonCloud.hasExpired && _poisonCloud.size != _poisonCloud.maxSize)
		{
			int stacks = otherPoisonCloud.stacks;
			(otherPoisonCloud.mapVisual as PoisonCloudMapObjectVisual).DoMove(base.transform.position, 4f);
			otherPoisonCloud.SetDoExpireEffect(state: false);
			otherPoisonCloud.Neutralize();
			_poisonCloud.SetStacks(_poisonCloud.stacks + stacks);
		}
	}

	private void AddTraitable(ITraitable obj)
	{
		if (obj is TileObject item)
		{
			if (!_objsInRange.Contains(item))
			{
				_objsInRange.Add(item);
				OnAddPOI(obj);
			}
		}
		else if (obj is Character character && !_charactersInRange.Contains(character))
		{
			_charactersInRange.Add(character);
			OnAddCharacter(character);
		}
	}

	private void RemoveTraitable(ITraitable obj)
	{
		if (obj is TileObject item)
		{
			_objsInRange.Remove(item);
		}
		else if (obj is Character item2)
		{
			_charactersInRange.Remove(item2);
		}
	}

	private void OnAddCharacter(Character p_character)
	{
		if (!p_character.combatComponent.isInCombat && p_character.isNormalCharacter)
		{
			p_character.combatComponent.Flight(_poisonCloud, "Poison_Cloud");
		}
		OnAddPOI(p_character);
	}

	private void OnAddPOI(ITraitable obj)
	{
		if (obj.traitContainer.GetTraitOrStatus<Trait>("Burning") != null)
		{
			Explode();
		}
	}

	public void Expire()
	{
		if (base.isSpawned)
		{
			_poisonCloud.OnExpire();
			_cloudEffect.Stop();
			visionTrigger.SetAllCollidersState(state: false);
			base.isSpawned = false;
			if (!string.IsNullOrEmpty(_expiryKey))
			{
				SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
			}
			Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
			Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
			_poisonCloud.Expire();
			StartCoroutine(DestroyCoroutine());
		}
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor2Seconds;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_cloudEffect.Play();
		_explosionEffect.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_cloudEffect.Pause();
		_explosionEffect.Pause();
	}

	public void SetSize(int size)
	{
		_size = size;
		ChangeScaleBySize();
	}

	private void ChangeScaleBySize()
	{
		Vector3 endValue = new Vector3(_size, _size, _size);
		base.transform.DOScale(endValue, 1f);
		_cloudEffect.transform.DOScale(endValue, 1f);
	}
}
