using Traits;
using UtilityScripts;

namespace Plague.Death_Effect;

public abstract class PlagueDeathEffect : Plagued.IPlagueDeathListener
{
	protected int _level;

	public abstract PLAGUE_DEATH_EFFECT deathEffectType { get; }

	public int level => _level;

	protected abstract void ActivateEffect(Character p_character);

	protected abstract int GetNextLevelUpgradeCost();

	public int GetFinalNextLevelUpgradeCost()
	{
		return SpellUtilities.GetModifiedSpellCost(GetNextLevelUpgradeCost(), WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}

	public abstract string GetCurrentEffectDescription();

	protected void ActivateEffectOn(Character p_character)
	{
		if (CanActivateEffectOn(p_character))
		{
			ActivateEffect(p_character);
		}
	}

	public PlagueDeathEffect()
	{
		_level = 1;
	}

	public virtual void OnDeath(Character p_character)
	{
	}

	protected virtual bool CanActivateEffectOn(Character p_character)
	{
		if (p_character.traitContainer.HasTrait("Plague Reservoir") || p_character.characterClass.IsZombie())
		{
			return false;
		}
		return true;
	}

	public void AdjustLevel(int amount)
	{
		_level += amount;
	}

	public void SetLevel(int amount)
	{
		_level = amount;
	}
}
