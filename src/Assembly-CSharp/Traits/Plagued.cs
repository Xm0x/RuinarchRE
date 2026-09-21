using System;
using System.Collections.Generic;
using Plague.Death_Effect;
using Plague.Fatality;
using Plague.Symptom;
using Plague.Transmission;
using UnityEngine;

namespace Traits;

public class Plagued : Status
{
	public interface IPlaguedListener
	{
		void PerTickWhileStationaryOrUnoccupied(Character p_character);

		void CharacterGainedTrait(Character p_character, Trait p_gainedTrait);

		void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action);

		void CharacterDonePerformingAction(Character p_character, INTERACTION_TYPE p_actionPerformed);

		void HourStarted(Character p_character, int numberOfHours);
	}

	public interface IPlagueDeathListener
	{
		void OnDeath(Character p_character);
	}

	private Action<Character> _perTickWhileStationaryOrUnoccupied;

	private Action<Character, Trait> _characterGainedTrait;

	private Action<Character, ActualGoapNode> _characterStartedPerformingAction;

	private Action<Character, int> _hourStarted;

	private Action<Character, INTERACTION_TYPE> _characterDonePerformingAction;

	private Action<Character> _characterDeath;

	private int _numberOfHoursPassed;

	private GameObject _infectedEffectGO;

	public IPointOfInterest owner { get; private set; }

	public override bool isPersistent => true;

	public int numberOfHoursPassed => _numberOfHoursPassed;

	public override Type serializedData => typeof(SaveDataPlagued);

	public override bool shouldBeLoadedInMainThread => true;

	public Plagued()
	{
		name = "Plagued";
		description = "Has a terrible and virulent disease.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.CURE_CHARACTER,
			INTERACTION_TYPE.HEALER_CURE
		};
		mutuallyExclusive = new string[1] { "Robust" };
		moodEffect = -4;
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Hour_Started_Trait");
		AddTraitOverrideFunctionIdentifier("Start_Perform_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Death_Trait");
		AddTraitOverrideFunctionIdentifier("After_Death");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataPlagued saveDataPlagued = saveDataTrait as SaveDataPlagued;
		_numberOfHoursPassed = saveDataPlagued.numberOfHoursPassed;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest pointOfInterest)
		{
			owner = pointOfInterest;
		}
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if ((bool)_infectedEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
			_infectedEffectGO = null;
		}
		_infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, allowRotation: false);
		if (owner is Character)
		{
			Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		}
		Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
		Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
		Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
		Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);
		for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++)
		{
			Fatality p_fatality = PlagueDisease.Instance.activeFatalities[i];
			AddFatality(p_fatality);
		}
		for (int j = 0; j < PlagueDisease.Instance.activeSymptoms.Count; j++)
		{
			PlagueSymptom p_symptom = PlagueDisease.Instance.activeSymptoms[j];
			AddSymptom(p_symptom);
		}
		AddDeathEffect(PlagueDisease.Instance.activeDeathEffect);
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (!(addedTo is IPointOfInterest pointOfInterest))
		{
			return;
		}
		owner = pointOfInterest;
		if ((bool)_infectedEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
			_infectedEffectGO = null;
		}
		_infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(owner, PARTICLE_EFFECT.Infected, allowRotation: false);
		if (pointOfInterest is Character character)
		{
			Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
			if (!character.traitContainer.HasTrait("Plague Reservoir") && PlayerManager.Instance.player.currenciesComponent.CanGainPlaguePoints())
			{
				if (character.isNormalCharacter)
				{
					PlayerManager.Instance.player?.currenciesComponent.GainPlaguePointFromCharacter(5, character);
				}
				else if (character is Summon summon)
				{
					if (summon.summonType != SUMMON_TYPE.Rat)
					{
						PlayerManager.Instance.player?.currenciesComponent.GainPlaguePointFromCharacter(1, character);
					}
				}
				else
				{
					PlayerManager.Instance.player?.currenciesComponent.GainPlaguePointFromCharacter(1, character);
				}
			}
			if (!character.isDead && character.isVillager)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<RatmenEvent>().AdjustNumberOfPlaguedVillagers(1);
			}
		}
		Messenger.AddListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
		Messenger.AddListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
		Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
		Messenger.AddListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);
		for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++)
		{
			Fatality p_fatality = PlagueDisease.Instance.activeFatalities[i];
			AddFatality(p_fatality);
		}
		for (int j = 0; j < PlagueDisease.Instance.activeSymptoms.Count; j++)
		{
			PlagueSymptom p_symptom = PlagueDisease.Instance.activeSymptoms[j];
			AddSymptom(p_symptom);
		}
		PlagueDisease.Instance.UpdateActiveCasesOnPOIGainedPlagued(pointOfInterest);
		AddDeathEffect(PlagueDisease.Instance.activeDeathEffect);
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if ((bool)_infectedEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
			_infectedEffectGO = null;
		}
		if (removedFrom is IPointOfInterest p_poi)
		{
			if (removedFrom is Character)
			{
				Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
			}
			Messenger.RemoveListener<Fatality>(PlayerSignals.ADDED_PLAGUE_DISEASE_FATALITY, OnPlagueDiseaseFatalityAdded);
			Messenger.RemoveListener<PlagueSymptom>(PlayerSignals.ADDED_PLAGUE_DISEASE_SYMPTOM, OnPlagueDiseaseSymptomAdded);
			Messenger.RemoveListener<PlagueDeathEffect>(PlayerSignals.SET_PLAGUE_DEATH_EFFECT, OnSetPlagueDeathEffect);
			Messenger.RemoveListener<PlagueDeathEffect>(PlayerSignals.UNSET_PLAGUE_DEATH_EFFECT, OnUnsetPlagueDeathEffect);
			for (int i = 0; i < PlagueDisease.Instance.activeFatalities.Count; i++)
			{
				Fatality p_fatality = PlagueDisease.Instance.activeFatalities[i];
				RemoveFatality(p_fatality);
			}
			for (int j = 0; j < PlagueDisease.Instance.activeSymptoms.Count; j++)
			{
				PlagueSymptom p_symptom = PlagueDisease.Instance.activeSymptoms[j];
				RemoveSymptom(p_symptom);
			}
			if (removedFrom is Character { isDead: false } character)
			{
				PlagueDisease.Instance.UpdateActiveCasesOnPOILostPlagued(p_poi);
				if (!character.characterClass.IsZombie())
				{
					PlagueDisease.Instance.UpdateRecoveriesOnPOILostPlagued(p_poi);
				}
				if (character.isVillager)
				{
					PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<RatmenEvent>().AdjustNumberOfPlaguedVillagers(-1);
				}
			}
			RemoveDeathEffect(PlagueDisease.Instance.activeDeathEffect);
		}
		owner = null;
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (owner is Character obj)
		{
			_perTickWhileStationaryOrUnoccupied?.Invoke(obj);
		}
		return false;
	}

	public override void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode p_actionNode)
	{
		base.ExecuteActionPreEffects(action, p_actionNode);
		IPointOfInterest otherObjectInAction = GetOtherObjectInAction(p_actionNode);
		if (otherObjectInAction is StructureTileObject || otherObjectInAction is GenericTileObject)
		{
			return;
		}
		switch (p_actionNode.action.actionCategory)
		{
		case ACTION_CATEGORY.CONSUME:
			if (!otherObjectInAction.traitContainer.HasTrait("Plagued"))
			{
				Transmission<ConsumptionTransmission>.Instance.Transmit(owner, otherObjectInAction, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Consumption));
			}
			break;
		case ACTION_CATEGORY.DIRECT:
			if (!otherObjectInAction.traitContainer.HasTrait("Plagued"))
			{
				Transmission<PhysicalContactTransmission>.Instance.Transmit(owner, otherObjectInAction, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Physical_Contact));
			}
			break;
		case ACTION_CATEGORY.VERBAL:
			if (p_actionNode.actor == owner)
			{
				Transmission<AirborneTransmission>.Instance.Transmit(owner, null, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
			}
			break;
		case ACTION_CATEGORY.INDIRECT:
			break;
		}
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is IPointOfInterest poi)
		{
			if ((bool)_infectedEffectGO)
			{
				ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
				_infectedEffectGO = null;
			}
			_infectedEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Infected, allowRotation: false);
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if ((bool)_infectedEffectGO)
		{
			ObjectPoolManager.Instance.DestroyObject(_infectedEffectGO);
			_infectedEffectGO = null;
		}
	}

	public override void OnHourStarted(ITraitable traitable)
	{
		base.OnHourStarted(traitable);
		_numberOfHoursPassed++;
		if (traitable is Character arg)
		{
			_hourStarted?.Invoke(arg, _numberOfHoursPassed);
		}
	}

	public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		if (owner.traitContainer.HasTrait("Plague Reservoir"))
		{
			return false;
		}
		if (node.actor == owner && owner is Character character)
		{
			if (character.characterClass.IsZombie())
			{
				return false;
			}
			if (_characterStartedPerformingAction != null)
			{
				_characterStartedPerformingAction(character, node);
				if (character.interruptComponent.isInterrupted && character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Total_Organ_Failure)
				{
					willStillContinueAction = false;
				}
				if (node.associatedJobType.IsHappinessRecoveryTypeJob() && !character.limiterComponent.canDoHappinessRecovery)
				{
					if (node.actor.jobQueue.jobsInQueue.Count > 0)
					{
						node.actor.jobQueue.jobsInQueue[0].CancelJob();
					}
					willStillContinueAction = false;
					if (owner.traitContainer.HasTrait("Depressed"))
					{
						Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " depressed", LOG_TAG.Life_Changes);
						log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
						log.AddLogToDatabase(releaseLogAfter: true);
					}
				}
				else if (node.associatedJobType.IsTirednessRecoveryTypeJob() && !character.limiterComponent.canDoTirednessRecovery)
				{
					if (node.actor.jobQueue.jobsInQueue.Count > 0)
					{
						node.actor.jobQueue.jobsInQueue[0].CancelJob();
					}
					willStillContinueAction = false;
					if (owner.traitContainer.HasTrait("Insomnia"))
					{
						Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " insomnia", LOG_TAG.Life_Changes);
						log2.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
						log2.AddLogToDatabase(releaseLogAfter: true);
					}
				}
				return true;
			}
		}
		return base.OnStartPerformGoapAction(node, ref willStillContinueAction);
	}

	public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved)
	{
		if (actor == owner && owner is Character arg)
		{
			_characterDonePerformingAction?.Invoke(arg, action);
		}
		base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
	}

	public override void AfterDeath(Character character)
	{
		_characterDeath?.Invoke(character);
	}

	public override bool OnDeath(Character character)
	{
		if (!character.characterClass.IsZombie())
		{
			if (PlayerManager.Instance.player.currenciesComponent.CanGainPlaguePoints())
			{
				PlayerManager.Instance.player.currenciesComponent.GainPlaguePointFromCharacter(2, character);
			}
			PlagueDisease.Instance.UpdateActiveCasesOnCharacterDied(character);
		}
		return base.OnDeath(character);
	}

	protected override string GetDescriptionInUI()
	{
		return base.GetDescriptionInUI() + "\n" + PlagueDisease.Instance.GetPlagueEffectsSummary();
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Plagued plagued)
		{
			_numberOfHoursPassed = plagued.numberOfHoursPassed;
		}
	}

	public void AddFatality(Fatality p_fatality)
	{
		SubscribeToAllPlagueListenerEvents(p_fatality);
	}

	public void RemoveFatality(Fatality p_fatality)
	{
		UnsubscribeToAllPlagueListenerEvents(p_fatality);
	}

	public void AddSymptom(PlagueSymptom p_symptom)
	{
		SubscribeToAllPlagueListenerEvents(p_symptom);
	}

	public void RemoveSymptom(PlagueSymptom p_symptom)
	{
		UnsubscribeToAllPlagueListenerEvents(p_symptom);
	}

	public void AddDeathEffect(PlagueDeathEffect p_deathEffect)
	{
		if (p_deathEffect != null)
		{
			SubscribeToDeathEffect(p_deathEffect);
		}
	}

	public void RemoveDeathEffect(PlagueDeathEffect p_deathEffect)
	{
		if (p_deathEffect != null)
		{
			UnsubscribeToDeathEffect(p_deathEffect);
		}
	}

	private void SubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener)
	{
		SubscribeToPerTickWhileStationaryOrUnoccupied(p_plaguedListener);
		SubscribeToCharacterGainedTrait(p_plaguedListener);
		SubscribeToCharacterStartedPerformingAction(p_plaguedListener);
		SubscribeToHourStarted(p_plaguedListener);
		SubscribeToCharacterDonePerformingAction(p_plaguedListener);
	}

	private void UnsubscribeToAllPlagueListenerEvents(IPlaguedListener p_plaguedListener)
	{
		UnsubscribeToPerTickWhileStationaryOrUnoccupied(p_plaguedListener);
		UnsubscribeToCharacterGainedTrait(p_plaguedListener);
		UnsubscribeToCharacterStartedPerformingAction(p_plaguedListener);
		UnsubscribeToCharacterDonePerformingAction(p_plaguedListener);
	}

	private void SubscribeToPerTickWhileStationaryOrUnoccupied(IPlaguedListener plaguedListener)
	{
		_perTickWhileStationaryOrUnoccupied = (Action<Character>)Delegate.Combine(_perTickWhileStationaryOrUnoccupied, new Action<Character>(plaguedListener.PerTickWhileStationaryOrUnoccupied));
	}

	private void UnsubscribeToPerTickWhileStationaryOrUnoccupied(IPlaguedListener plaguedListener)
	{
		_perTickWhileStationaryOrUnoccupied = (Action<Character>)Delegate.Remove(_perTickWhileStationaryOrUnoccupied, new Action<Character>(plaguedListener.PerTickWhileStationaryOrUnoccupied));
	}

	private void SubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener)
	{
		_characterGainedTrait = (Action<Character, Trait>)Delegate.Combine(_characterGainedTrait, new Action<Character, Trait>(plaguedListener.CharacterGainedTrait));
	}

	private void UnsubscribeToCharacterGainedTrait(IPlaguedListener plaguedListener)
	{
		_characterGainedTrait = (Action<Character, Trait>)Delegate.Remove(_characterGainedTrait, new Action<Character, Trait>(plaguedListener.CharacterGainedTrait));
	}

	private void SubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener)
	{
		_characterStartedPerformingAction = (Action<Character, ActualGoapNode>)Delegate.Combine(_characterStartedPerformingAction, (Action<Character, ActualGoapNode>)delegate(Character pCharacter, ActualGoapNode pAction)
		{
			plaguedListener.CharacterStartedPerformingAction(pCharacter, pAction);
		});
	}

	private void UnsubscribeToCharacterStartedPerformingAction(IPlaguedListener plaguedListener)
	{
		_characterStartedPerformingAction = (Action<Character, ActualGoapNode>)Delegate.Remove(_characterStartedPerformingAction, (Action<Character, ActualGoapNode>)delegate(Character pCharacter, ActualGoapNode pAction)
		{
			plaguedListener.CharacterStartedPerformingAction(pCharacter, pAction);
		});
	}

	private void SubscribeToHourStarted(IPlaguedListener p_plaguedListener)
	{
		_hourStarted = (Action<Character, int>)Delegate.Combine(_hourStarted, new Action<Character, int>(p_plaguedListener.HourStarted));
	}

	private void UnsubscribeToHourStarted(IPlaguedListener p_plaguedListener)
	{
		_hourStarted = (Action<Character, int>)Delegate.Remove(_hourStarted, new Action<Character, int>(p_plaguedListener.HourStarted));
	}

	private void SubscribeToCharacterDonePerformingAction(IPlaguedListener p_plaguedListener)
	{
		_characterDonePerformingAction = (Action<Character, INTERACTION_TYPE>)Delegate.Combine(_characterDonePerformingAction, new Action<Character, INTERACTION_TYPE>(p_plaguedListener.CharacterDonePerformingAction));
	}

	private void UnsubscribeToCharacterDonePerformingAction(IPlaguedListener p_plaguedListener)
	{
		_characterDonePerformingAction = (Action<Character, INTERACTION_TYPE>)Delegate.Remove(_characterDonePerformingAction, new Action<Character, INTERACTION_TYPE>(p_plaguedListener.CharacterDonePerformingAction));
	}

	private void SubscribeToDeathEffect(IPlagueDeathListener p_plaguedDeathListener)
	{
		_characterDeath = (Action<Character>)Delegate.Combine(_characterDeath, new Action<Character>(p_plaguedDeathListener.OnDeath));
	}

	private void UnsubscribeToDeathEffect(IPlagueDeathListener p_plaguedDeathListener)
	{
		_characterDeath = (Action<Character>)Delegate.Remove(_characterDeath, new Action<Character>(p_plaguedDeathListener.OnDeath));
	}

	private void OnPlagueDiseaseFatalityAdded(Fatality p_fatality)
	{
		AddFatality(p_fatality);
	}

	private void OnPlagueDiseaseSymptomAdded(PlagueSymptom p_symptom)
	{
		AddSymptom(p_symptom);
	}

	private void OnSetPlagueDeathEffect(PlagueDeathEffect p_deathEffect)
	{
		AddDeathEffect(p_deathEffect);
	}

	private void OnUnsetPlagueDeathEffect(PlagueDeathEffect p_deathEffect)
	{
		RemoveDeathEffect(p_deathEffect);
	}

	private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait)
	{
		if (p_traitable == owner && owner is Character arg)
		{
			_characterGainedTrait?.Invoke(arg, p_trait);
		}
	}

	private IPointOfInterest GetOtherObjectInAction(ActualGoapNode p_actionNode)
	{
		if (p_actionNode.actor != owner)
		{
			return p_actionNode.actor;
		}
		return p_actionNode.target;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
