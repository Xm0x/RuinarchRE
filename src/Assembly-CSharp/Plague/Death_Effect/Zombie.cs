using UtilityScripts;

namespace Plague.Death_Effect;

public class Zombie : PlagueDeathEffect
{
	public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Zombie;

	protected override void ActivateEffect(Character p_character)
	{
		switch (_level)
		{
		case 1:
			WalkerZombie(p_character);
			break;
		case 2:
			NightZombie(p_character);
			break;
		case 3:
			VarietyZombie(p_character);
			break;
		}
	}

	protected override int GetNextLevelUpgradeCost()
	{
		return _level switch
		{
			1 => 50, 
			2 => 75, 
			_ => -1, 
		};
	}

	public override string GetCurrentEffectDescription()
	{
		return _level switch
		{
			1 => LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Walker Zombie"), 
			2 => LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Night Zombie"), 
			3 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Variety Zombie"), 
			_ => string.Empty, 
		};
	}

	public override void OnDeath(Character p_character)
	{
		ActivateEffectOn(p_character);
	}

	private void WalkerZombie(Character p_character)
	{
		if (!p_character.characterClass.IsZombie())
		{
			p_character.visuals.UsePreviousClassAsset(p_state: true);
			p_character.classComponent.AssignClass("Walker Zombie");
		}
	}

	private void NightZombie(Character p_character)
	{
		if (!p_character.characterClass.IsZombie())
		{
			p_character.visuals.UsePreviousClassAsset(p_state: true);
			p_character.classComponent.AssignClass("Night Zombie");
		}
	}

	private void VarietyZombie(Character p_character)
	{
		if (!p_character.characterClass.IsZombie())
		{
			p_character.visuals.UsePreviousClassAsset(p_state: true);
			string className = "Boomer Zombie";
			int num = GameUtilities.RandomBetweenTwoNumbers(0, 99);
			if (num >= 0 && num < 25)
			{
				className = "Walker Zombie";
			}
			else if (num >= 25 && num < 50)
			{
				className = "Fast Zombie";
			}
			else if (num >= 50 && num < 70)
			{
				className = "Night Zombie";
			}
			else if (num >= 70 && num < 85)
			{
				className = "Boomer Zombie";
			}
			else if (num >= 85 && num < 100)
			{
				className = "Tank Zombie";
			}
			p_character.classComponent.AssignClass(className);
		}
	}
}
