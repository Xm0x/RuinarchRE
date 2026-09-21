using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class ElectricStormTileObject : AOESpellTileObject
{
	private float _timeInterval;

	private float _currentTime;

	public bool isElectricStormCastedByPlayer { get; private set; }

	public int currentElectricStormDuration { get; private set; }

	public override Type serializedData => typeof(SaveDataElectricStormTileObject);

	protected override int effectRadius => 6;

	public ElectricStormTileObject()
		: base(TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT)
	{
		currentElectricStormDuration = 0;
	}

	public ElectricStormTileObject(SaveDataTileObject data)
		: base(data)
	{
		SaveDataElectricStormTileObject saveDataElectricStormTileObject = data as SaveDataElectricStormTileObject;
		isElectricStormCastedByPlayer = saveDataElectricStormTileObject.isElectricStormCastedByPlayer;
		currentElectricStormDuration = saveDataElectricStormTileObject.currentElectricStormDuration;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		StartElectricStorm();
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		StopElectricStorm();
	}

	private void StartElectricStorm()
	{
		GameManager.Instance.StartCoroutine(CommenceElectricStorm());
		Messenger.AddListener(Signals.TICK_STARTED, PerTickElectricStorm);
	}

	private void StopElectricStorm()
	{
		GameManager.Instance.StopCoroutine(CommenceElectricStorm());
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickElectricStorm);
	}

	private IEnumerator CommenceElectricStorm()
	{
		SkillData electricStormData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.ELECTRIC_STORM);
		float piercing = 0f;
		int processedDamage;
		if (!isElectricStormCastedByPlayer)
		{
			processedDamage = PlayerSkillManager.Instance.GetDamageBaseOnLevel(electricStormData, 0);
		}
		else
		{
			processedDamage = PlayerSkillManager.Instance.GetDamageBaseOnLevel(electricStormData);
			piercing = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(electricStormData);
		}
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
					GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(randomElement, PARTICLE_EFFECT.Lightning_Strike);
					AkSoundEngine.PostEvent("Play_Electric_Storm", in_gameObjectID);
					randomElement.tileObjectComponent.genericTileObject.traitContainer.AddTrait(randomElement.tileObjectComponent.genericTileObject, "Danger Remnant", out var trait);
					if (trait is RemnantTrait remnantTrait)
					{
						remnantTrait.SetSpellUsed(PLAYER_SKILL_TYPE.ELECTRIC_STORM);
					}
					List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
					randomElement.PopulateAliveTraitablesOnTile(list);
					for (int i = 0; i < list.Count; i++)
					{
						ITraitable traitable = list[i];
						ElectricStormEffect(traitable, processedDamage, piercing, electricStormData);
					}
					RuinarchListPool<ITraitable>.Release(list);
					RandomizeTimeInterval();
					_currentTime = 0f;
				}
			}
			yield return null;
		}
	}

	private void ElectricStormEffect(ITraitable traitable, int processedDamage, float piercing, SkillData electricStormData)
	{
		traitable.AdjustHP(-processedDamage, ELEMENTAL_TYPE.Electric, triggerDeath: true, piercingPower: piercing, isPlayerSource: isElectricStormCastedByPlayer, source: isElectricStormCastedByPlayer ? electricStormData : null, elementalTraitProcessor: null, showHPBar: true);
		if (traitable is Character character && isElectricStormCastedByPlayer)
		{
			character.OnCharacterHitByPlayerSpell(-processedDamage);
		}
	}

	private void RandomizeTimeInterval()
	{
		_timeInterval = UnityEngine.Random.Range(0.1f, 0.7f);
	}

	private void PerTickElectricStorm()
	{
		currentElectricStormDuration++;
		if (isElectricStormCastedByPlayer)
		{
			if (currentElectricStormDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ELECTRIC_STORM))
			{
				gridTileLocation.structure.RemovePOI(this);
			}
		}
		else if (currentElectricStormDuration >= PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.ELECTRIC_STORM, 0))
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public void ResetElectricStormDuration()
	{
		currentElectricStormDuration = 0;
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isElectricStormCastedByPlayer = p_state;
	}
}
