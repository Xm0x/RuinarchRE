using System.Collections;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class StampedeMapObjectVisual : MovingMapObjectVisual
{
	[SerializeField]
	private SpriteRenderer[] _stampedeSpriteRenderers;

	[SerializeField]
	private GameObject[] _stampedeLevelGOs;

	[SerializeField]
	private ParticleSystem[] _dustTrails;

	[SerializeField]
	private ParticleSystem[] _dustSmokes;

	[SerializeField]
	private BoxCollider2D _stampedeVisionCollider;

	[SerializeField]
	private BoxCollider2D _stampedeVisionTrigger;

	[SerializeField]
	private BoxCollider2D _stampedeProjectileReceiver;

	private ParticleSystem _stampedeParticleDustTrail;

	private ParticleSystem _stampedeParticleDustSmoke;

	private string _expiryKey;

	private Stampede owner;

	private uint _sfxID;

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
		owner = obj as Stampede;
		_sfxID = AudioManager.Instance.PlaySpellSFXAndUpdateBasedOnTimeState("Play_Stampede", base.gameObject);
	}

	public override void Rotate(Quaternion target, bool force = false)
	{
		base.Rotate(target, force);
		base.transform.rotation = target;
	}

	public void StartMovement()
	{
		StartMoving(owner.targetDirection, owner.targetDirection == Vector3.zero);
	}

	private void StartMoving(Vector3 p_targetDirection, bool p_moveToRandom)
	{
		float p_speed = PlayerSkillManager.Instance.GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE.STAMPEDE);
		if (!p_moveToRandom)
		{
			MoveToDirection(p_targetDirection, p_speed, 200f);
		}
		else
		{
			MoveToRandomDirectionInitial(p_speed, 200f);
		}
		if (string.IsNullOrEmpty(_expiryKey))
		{
			owner.expiryDate = GameManager.Instance.Today().AddTicks(PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.STAMPEDE));
			ScheduleExpiry(owner.expiryDate);
		}
		base.isSpawned = true;
		if (GameManager.Instance.isPaused || !GameManager.Instance.gameHasStarted)
		{
			StartCoroutine(PlayParticleCoroutineWhenGameIsPaused());
		}
	}

	public void ScheduleExpiry(GameDate p_expiryDate)
	{
		if (string.IsNullOrEmpty(_expiryKey))
		{
			_expiryKey = SchedulingManager.Instance.AddEntry(p_expiryDate, Expire, this);
		}
	}

	public void UpdateWidth()
	{
		int width = owner.width;
		int levelByWidth = (PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.STAMPEDE) as StampedeData).GetLevelByWidth(width);
		_stampedeVisionCollider.size = new Vector2(width, 2f);
		_stampedeVisionTrigger.size = new Vector2(width, 2f);
		_stampedeProjectileReceiver.size = new Vector2(width, 2f);
		_stampedeLevelGOs[levelByWidth].SetActive(value: true);
		_stampedeParticleDustTrail = _dustTrails[levelByWidth];
		_stampedeParticleDustSmoke = _dustSmokes[levelByWidth];
	}

	public override void Reset()
	{
		base.Reset();
		base.isSpawned = false;
		_expiryKey = string.Empty;
		_movement?.Kill();
		_movement = null;
		_stampedeParticleDustTrail.Clear();
		_stampedeParticleDustTrail.Stop();
		_stampedeParticleDustTrail = null;
		_stampedeParticleDustSmoke.Clear();
		_stampedeParticleDustSmoke.Stop();
		_stampedeParticleDustSmoke = null;
		base.transform.rotation = Quaternion.identity;
		for (int i = 0; i < _stampedeLevelGOs.Length; i++)
		{
			_stampedeLevelGOs[i].SetActive(value: false);
		}
		AkSoundEngine.PostEvent("Stop_Stampede", base.gameObject);
	}

	protected override void OnGamePaused(bool isPaused)
	{
		base.OnGamePaused(isPaused);
		if (isPaused)
		{
			_stampedeParticleDustTrail.Pause();
			_stampedeParticleDustSmoke.Pause();
		}
		else
		{
			_stampedeParticleDustTrail.Play();
			_stampedeParticleDustSmoke.Play();
		}
	}

	protected override void UpdateMovementSpeedGivenProgression(PROGRESSION_SPEED p_progression)
	{
		if (_movement != null)
		{
			switch (p_progression)
			{
			case PROGRESSION_SPEED.X1:
				_movement.timeScale = 1f;
				break;
			case PROGRESSION_SPEED.X2:
				_movement.timeScale = 1.5f;
				break;
			case PROGRESSION_SPEED.X4:
				_movement.timeScale = 2.9f;
				break;
			}
		}
	}

	private void DealDamageToTraitable(ITraitable p_traitable)
	{
		if (!base.isSpawned)
		{
			return;
		}
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.STAMPEDE);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		if (owner != p_traitable)
		{
			if (p_traitable is Character character)
			{
				if (!character.isDead)
				{
					p_traitable.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Normal, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
					if (owner.isPlayerSource)
					{
						character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
					}
					if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
					{
						character.skillCauseOfDeath = PLAYER_SKILL_TYPE.STAMPEDE;
					}
					if (!character.isDead)
					{
						character.traitContainer.AddTrait(character, "Unconscious");
					}
				}
			}
			else
			{
				p_traitable.AdjustHP(-damageBaseOnLevel, ELEMENTAL_TYPE.Normal, triggerDeath: true, piercingPower: pierceBasedOnCurrentLevel, isPlayerSource: owner.isPlayerSource, source: owner.isPlayerSource ? spellData : null, elementalTraitProcessor: null, showHPBar: true);
			}
		}
		LocationGridTile locationGridTile = base.gridTileLocation;
		if (locationGridTile != null && locationGridTile.structure != null)
		{
			locationGridTile.structure.OnTileDamaged(locationGridTile, -damageBaseOnLevel, owner.isPlayerSource);
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (base.isSpawned)
		{
			BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
			if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is ITraitable p_traitable && GameManager.Instance.gameHasStarted)
			{
				DealDamageToTraitable(p_traitable);
			}
		}
	}

	public void Expire()
	{
		_stampedeParticleDustTrail.Stop();
		_stampedeParticleDustSmoke.Stop();
		base.isSpawned = false;
		if (!string.IsNullOrEmpty(_expiryKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
			_expiryKey = string.Empty;
		}
		owner.Expire();
		StartCoroutine(DestroyCoroutine());
	}

	private IEnumerator DestroyCoroutine()
	{
		yield return GameUtilities.waitFor1Second;
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	private IEnumerator PlayParticleCoroutineWhenGameIsPaused()
	{
		_stampedeParticleDustTrail.Play();
		_stampedeParticleDustSmoke.Play();
		yield return GameUtilities.waitForTenthOfSecond;
		_stampedeParticleDustTrail.Pause();
		_stampedeParticleDustSmoke.Pause();
	}
}
