using System;
using System.Collections.Generic;
using Inner_Maps;
using Interrupts;
using Traits;
using UtilityScripts;

public class Sludge : Summon
{
	private Action<ITraitable> _traitableCallback;

	public Sludge()
		: base(SUMMON_TYPE.Sludge, "Sludge", RACE.SLUDGE, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
		_traitableCallback = ApplyDamageTo;
	}

	public Sludge(string className)
		: base(SUMMON_TYPE.Sludge, className, RACE.SLUDGE, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
		_traitableCallback = ApplyDamageTo;
	}

	public Sludge(SaveDataSummon data)
		: base(data)
	{
		_traitableCallback = ApplyDamageTo;
	}

	public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null, Interrupt interrupt = null, bool isPlayerSource = false, object deathSource = null, ELEMENTAL_TYPE elementType = ELEMENTAL_TYPE.Normal)
	{
		if (!base.isDead)
		{
			LocationGridTile locationGridTile = base.gridTileLocation;
			base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller, interrupt, isPlayerSource, deathSource, ELEMENTAL_TYPE.Normal);
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

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		base.jobComponent.TriggerSpawnPoisonCloud(JOB_TYPE.AGITATED, 4, 10, out p_agitateJob);
		if (p_agitateJob is GoapPlanJob goapPlanJob)
		{
			goapPlanJob.SetIsAgitateJob(p_state: true);
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
			return true;
		}
		return false;
	}

	private void ApplyDamageTo(ITraitable traitable)
	{
		if (!(traitable is Character))
		{
			traitable.AdjustHP(-50, base.combatComponent.currentElement.type, triggerDeath: true, this);
		}
	}
}
