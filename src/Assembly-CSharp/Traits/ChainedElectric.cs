using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class ChainedElectric : Status, IElementalTrait
{
	private float _currentTime;

	public int damage { get; private set; }

	public bool hasInflictedDamage { get; private set; }

	public ITraitable traitable { get; private set; }

	public bool isPlayerSource { get; private set; }

	public override Type serializedData => typeof(SaveDataChainedElectric);

	public override bool shouldBeLoadedInMainThread => true;

	public ChainedElectric()
	{
		name = "Chained Electric";
		description = "Affected by electric chain damage.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
		isHidden = true;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		traitable = sourcePOI;
		_currentTime = 0f;
		GameManager.Instance.StartCoroutine(InflictDamageEnumerator(traitable, 0.5f * GameManager.Instance.progressionSpeed));
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		GameManager.Instance.StopCoroutine(InflictDamageEnumerator(traitable, 0.5f * GameManager.Instance.progressionSpeed));
		traitable = null;
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI();
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataChainedElectric saveDataChainedElectric = saveDataTrait as SaveDataChainedElectric;
		damage = saveDataChainedElectric.damage;
		hasInflictedDamage = saveDataChainedElectric.hasInflictedDamage;
		isPlayerSource = saveDataChainedElectric.isPlayerSource;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (!hasInflictedDamage)
		{
			_currentTime = 0f;
			GameManager.Instance.StartCoroutine(InflictDamageEnumerator(traitable, 0.5f * GameManager.Instance.progressionSpeed));
		}
	}

	private IEnumerator InflictDamageEnumerator(ITraitable traitable, float p_timeInterval)
	{
		while (true)
		{
			if (!GameManager.Instance.isPaused && GameManager.Instance.gameHasStarted)
			{
				_currentTime += Time.deltaTime;
				if (_currentTime >= p_timeInterval)
				{
					break;
				}
			}
			yield return null;
		}
		InflictDamage(traitable);
	}

	public void SetDamage(int amount)
	{
		damage = amount;
	}

	private void InflictDamage(ITraitable traitable)
	{
		if (traitable == null || hasInflictedDamage)
		{
			return;
		}
		hasInflictedDamage = true;
		LocationGridTile gridTileLocation = traitable.gridTileLocation;
		if (gridTileLocation == null)
		{
			return;
		}
		int num = Mathf.RoundToInt((float)damage * 0.8f);
		if (num >= 0)
		{
			num = -1;
		}
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> neighbourList = gridTileLocation.neighbourList;
		for (int i = 0; i < neighbourList.Count; i++)
		{
			LocationGridTile locationGridTile = neighbourList[i];
			if (locationGridTile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Wet") && !locationGridTile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Zapped", "Chained Electric"))
			{
				locationGridTile.PopulateTraitablesOnTile(list);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			ITraitable traitable2 = list[j];
			ChainElectricEffect(traitable2, num, base.responsibleCharacter);
		}
		RuinarchListPool<ITraitable>.Release(list);
	}

	private void ChainElectricEffect(ITraitable traitable, int damage, Character responsibleCharacter)
	{
		if (traitable.gridTileLocation != null)
		{
			traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, triggerDeath: true, responsibleCharacter, null, showHPBar: true, 0f, isPlayerSource);
		}
	}

	public void SetIsPlayerSource(bool p_state)
	{
		isPlayerSource = p_state;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = traitable;
	}
}
