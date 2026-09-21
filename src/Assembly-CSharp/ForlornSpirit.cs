using System;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class ForlornSpirit : TileObject
{
	private SpiritGameObject _spiritGO;

	private int _duration;

	private int _currentDuration;

	public Character possessionTarget { get; private set; }

	public int currentDuration => _currentDuration;

	public override Type serializedData => typeof(SaveDataForlornSpirit);

	public ForlornSpirit()
	{
		_duration = GameManager.Instance.GetTicksBasedOnHour(1);
		Initialize(TILE_OBJECT_TYPE.FORLORN_SPIRIT, shouldAddCommonAdvertisements: false);
		base.traitContainer.AddTrait(this, "Forlorn");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public ForlornSpirit(SaveDataForlornSpirit data)
		: base(data)
	{
		_duration = GameManager.Instance.GetTicksBasedOnHour(1);
		_currentDuration = data.currentDuration;
	}

	public override string ToString()
	{
		return "Forlorn Spirit " + base.id;
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		Messenger.AddListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.AddListener<bool>(UISignals.PAUSED, OnGamePaused);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		_spiritGO.SetIsRoaming(!GameManager.Instance.isPaused, isInitial: true);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.RemoveListener<PROGRESSION_SPEED>(UISignals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
		Messenger.RemoveListener<bool>(UISignals.PAUSED, OnGamePaused);
		Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
	}

	protected override void CreateMapObjectVisual()
	{
		GameObject gameObject = InnerMapManager.Instance.mapObjectFactory.CreateNewTileObjectMapVisual(base.tileObjectType);
		_spiritGO = gameObject.GetComponent<SpiritGameObject>();
		mapVisual = _spiritGO;
	}

	private void OnProgressionSpeedChanged(PROGRESSION_SPEED prog)
	{
		_spiritGO.UpdateMovementSpeedGivenProgression(prog);
	}

	private void OnGamePaused(bool paused)
	{
		if (possessionTarget == null)
		{
			_spiritGO.SetIsRoaming(!paused);
		}
	}

	public void StartSpiritPossession(Character target)
	{
		if (possessionTarget == null)
		{
			possessionTarget = target;
			_spiritGO.SetIsRoaming(state: false);
			_spiritGO.StartInitialPossessTarget();
		}
	}

	public void FinishPossession()
	{
		AkSoundEngine.PostEvent("Play_Spirit_Hit", _spiritGO.gameObject);
		ForlornEffect();
		DonePossession();
	}

	private void OnTickEnded()
	{
		if (_spiritGO != null && _spiritGO.isRoaming)
		{
			_currentDuration++;
			if (_currentDuration >= _duration)
			{
				_spiritGO.SetIsRoaming(state: false);
				Dissipate();
			}
		}
	}

	private void ForlornEffect()
	{
		if (possessionTarget != null && !possessionTarget.isDead)
		{
			int p_value = 100;
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FORLORN_SPIRIT);
			RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.FORLORN_SPIRIT).resistanceType;
			float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
			float resistanceValue = possessionTarget.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
			CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
			if (GameUtilities.RollChance(p_value))
			{
				float adjustment = 0f - PlayerSkillManager.Instance.GetIncreaseStatsPercentagePerLevel(spellData);
				possessionTarget.needsComponent.AdjustHappiness(adjustment);
			}
			else
			{
				possessionTarget.reactionComponent.ResistRuinarchPower();
			}
		}
	}

	private void DonePossession()
	{
		GameManager.Instance.CreateParticleEffectAt(possessionTarget.gridTileLocation, PARTICLE_EFFECT.Minion_Dissipate);
		DestroySpirit();
	}

	public void Dissipate(LocationGridTile p_tile = null)
	{
		LocationGridTile locationGridTile = p_tile;
		if (locationGridTile == null)
		{
			locationGridTile = gridTileLocation;
		}
		if (locationGridTile != null)
		{
			GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Minion_Dissipate);
		}
		DestroySpirit();
	}

	private void DestroySpirit()
	{
		iTween.Stop(mapVisual.gameObject);
		SetGridTileLocation(null);
		OnDestroyPOI();
		possessionTarget = null;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (possessionTarget == p_character)
		{
			possessionTarget = null;
			_spiritGO.SetIsRoaming(state: true);
		}
	}
}
