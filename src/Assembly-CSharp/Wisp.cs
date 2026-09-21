using System;
using System.Collections.Generic;
using Inner_Maps;
using Interrupts;
using Traits;
using UtilityScripts;

public abstract class Wisp : Summon
{
	private Action<ITraitable> _traitableCallback;

	public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Aggressive;

	protected Wisp(SUMMON_TYPE summonType, string className)
		: base(summonType, className, RACE.WISP, Utilities.GetRandomGender())
	{
		base.visuals.SetHasBlood(state: false);
		_traitableCallback = ApplyDamageTo;
	}

	protected Wisp(SaveDataSummon data)
		: base(data)
	{
		_traitableCallback = ApplyDamageTo;
	}

	public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null, Interrupt interrupt = null, bool isPlayerSource = false, object deathSource = null, ELEMENTAL_TYPE elementType = ELEMENTAL_TYPE.Normal)
	{
		if (!base.isDead)
		{
			LocationGridTile locationGridTile = base.gridTileLocation;
			base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller, interrupt, isPlayerSource, (object)null, ELEMENTAL_TYPE.Normal);
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
			locationGridTile.PopulateTilesInRadius(list2, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list2.Count; i++)
			{
				list2[i].PopulateAliveTraitablesOnTile(list);
			}
			RuinarchListPool<LocationGridTile>.Release(list2);
			TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
			RuinarchListPool<ITraitable>.Release(list);
		}
	}

	public override void LoadReferencesMainThread(SaveDataCharacter data)
	{
		base.LoadReferencesMainThread(data);
		base.visuals.SetHasBlood(state: false);
	}

	private void ApplyDamageTo(ITraitable traitable)
	{
		if (traitable != this)
		{
			traitable.AdjustHP(-50, base.combatComponent.currentElement.type, triggerDeath: true, this);
		}
	}
}
