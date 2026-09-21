using System;
using UnityEngine;
using UtilityScripts;

namespace Plague.Transmission;

public abstract class Transmission<T> where T : Transmission<T>, new()
{
	private static readonly Lazy<T> Lazy = new Lazy<T>(() => Activator.CreateInstance(typeof(T), nonPublic: true) as T);

	private Action<IPointOfInterest> _plagueTransmitted;

	public static T Instance => Lazy.Value;

	public abstract PLAGUE_TRANSMISSION transmissionType { get; }

	protected abstract int GetTransmissionRate(int level);

	protected abstract int GetTransmissionNextLevelCost(int p_currentLevel);

	public int GetFinalTransmissionNextLevelCost(int p_currentLevel)
	{
		return SpellUtilities.GetModifiedSpellCost(GetTransmissionNextLevelCost(p_currentLevel), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public abstract void Transmit(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl);

	protected void TryTransmitToSingleTarget(IPointOfInterest p_infector, IPointOfInterest p_target, int p_transmissionLvl)
	{
		int transmissionRate = GetTransmissionRate(p_transmissionLvl);
		transmissionRate = AdjustTransmissionChancesBasedOnInfector(p_infector, transmissionRate);
		if (GameUtilities.RollChance(transmissionRate))
		{
			Infect(p_infector, p_target);
		}
	}

	protected void TryTransmitToInRange(IPointOfInterest p_infector, int p_transmissionLvl)
	{
		int transmissionRate = GetTransmissionRate(p_transmissionLvl);
		transmissionRate = AdjustTransmissionChancesBasedOnInfector(p_infector, transmissionRate);
		if (!(p_infector is Character { hasMarker: not false } character))
		{
			return;
		}
		for (int i = 0; i < character.marker.inVisionCharacters.Count; i++)
		{
			Character p_target = character.marker.inVisionCharacters[i];
			if (GameUtilities.RollChance(transmissionRate))
			{
				Infect(p_infector, p_target);
			}
		}
	}

	private void Infect(IPointOfInterest p_infector, IPointOfInterest p_target)
	{
		if (p_target is Character character)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, character);
		}
		else if (PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(p_target))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Plagued contract", LOG_TAG.Life_Changes);
			log.AddToFillers(p_target, p_target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		_plagueTransmitted?.Invoke(p_target);
	}

	private int AdjustTransmissionChancesBasedOnInfector(IPointOfInterest p_infector, int p_baseChance)
	{
		int num = p_baseChance;
		if (p_infector.traitContainer.HasTrait("Quarantined"))
		{
			num -= Mathf.FloorToInt((float)num * 0.75f);
		}
		return num;
	}

	public void SubscribeToTransmission(IPlagueTransmissionListener p_PlagueTransmissionListener)
	{
		_plagueTransmitted = (Action<IPointOfInterest>)Delegate.Combine(_plagueTransmitted, new Action<IPointOfInterest>(p_PlagueTransmissionListener.OnPlagueTransmitted));
	}

	public void UnsubscribeToTransmission(IPlagueTransmissionListener p_PlagueTransmissionListener)
	{
		_plagueTransmitted = (Action<IPointOfInterest>)Delegate.Remove(_plagueTransmitted, new Action<IPointOfInterest>(p_PlagueTransmissionListener.OnPlagueTransmitted));
	}
}
