using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

namespace Plague.Death_Effect;

public class Explosion : PlagueDeathEffect
{
	private Action<ITraitable> _traitableCallback;

	public override PLAGUE_DEATH_EFFECT deathEffectType => PLAGUE_DEATH_EFFECT.Explosion;

	protected override void ActivateEffect(Character p_character)
	{
		switch (_level)
		{
		case 1:
			FireBlast(p_character);
			break;
		case 2:
			FireBlastAndFireElementals(p_character);
			break;
		case 3:
			Meteor(p_character);
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
			1 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Fire Blast"), 
			2 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Fire Elemental"), 
			3 => LocalizationManager.Instance.GetLocalizedValue("Plague_Table", "Meteor"), 
			_ => string.Empty, 
		};
	}

	public override void OnDeath(Character p_character)
	{
		ActivateEffectOn(p_character);
	}

	private void Ignite(Character p_character)
	{
		if ((bool)p_character.marker)
		{
			BurningSource source = new BurningSource();
			Burning burning = TraitManager.Instance.CreateNewInstancedTraitClass<Burning>("Burning");
			burning.SetSourceOfBurning(source, p_character);
			p_character.traitContainer.AddTrait(p_character, burning, null, bypassElementalChance: true);
		}
	}

	private void FireBlast(Character p_character)
	{
		if (_traitableCallback == null)
		{
			_traitableCallback = FireBlastEffect;
		}
		LocationGridTile gridTileLocation = p_character.gridTileLocation;
		if (gridTileLocation != null)
		{
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			for (int i = 0; i < gridTileLocation.neighbourList.Count; i++)
			{
				gridTileLocation.neighbourList[i].PopulateAliveTraitablesOnTile(list);
			}
			TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
			RuinarchListPool<ITraitable>.Release(list);
		}
	}

	private void FireBlastEffect(ITraitable traitable)
	{
		if (traitable.gridTileLocation != null)
		{
			BurningSource burningSource = null;
			traitable.AdjustHP(-150, ELEMENTAL_TYPE.Fire, triggerDeath: true, null, delegate(ITraitable target, Trait trait)
			{
				TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource);
			}, showHPBar: true);
		}
	}

	private void FireBlastAndFireElementals(Character p_character)
	{
		FireBlast(p_character);
		LocationGridTile gridTileLocation = p_character.gridTileLocation;
		if (gridTileLocation != null)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Fire_Elemental, FactionManager.Instance.wildMonsterFaction, null, gridTileLocation.parentMap.region);
			CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
			summon.SetTerritory(gridTileLocation.area, returnHome: false);
		}
	}

	private void Meteor(Character p_character)
	{
		if (p_character.gridTileLocation != null)
		{
			p_character.gridTileLocation.AddMeteor();
		}
	}
}
