using System.Collections.Generic;
using Interrupts;
using Traits;
using UtilityScripts;

public class Splatter : Summon
{
	public override bool defaultDigMode => true;

	public Splatter()
		: base(SUMMON_TYPE.Splatter, "Splatter", RACE.SPLATTER, Utilities.GetRandomGender())
	{
	}

	public Splatter(string className)
		: base(SUMMON_TYPE.Splatter, className, RACE.SPLATTER, Utilities.GetRandomGender())
	{
	}

	public Splatter(SaveDataSummon data)
		: base(data)
	{
	}

	public override void Death(string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] multipleDeathLogFillers = null, LogFiller p_singleDeathLogFiller = null, Interrupt interrupt = null, bool isPlayerSource = false, object deathSource = null, ELEMENTAL_TYPE elementType = ELEMENTAL_TYPE.Normal)
	{
		base.Death(cause, deathFromAction, responsibleCharacter, _deathLog, multipleDeathLogFillers, p_singleDeathLogFiller, interrupt, isPlayerSource, deathSource, elementType);
		if (base.deathTilePosition == null)
		{
			return;
		}
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		base.deathTilePosition.PopulateAliveTraitablesOnTile(list);
		for (int i = 0; i < base.deathTilePosition.neighbourList.Count; i++)
		{
			base.deathTilePosition.neighbourList[i].PopulateAliveTraitablesOnTile(list);
		}
		if (elementType != ELEMENTAL_TYPE.Fire)
		{
			for (int j = 0; j < list.Count; j++)
			{
				ITraitable traitable = list[j];
				if (!(traitable is Character { faction: not null } character) || !character.faction.IsFriendlyWith(base.faction))
				{
					traitable.AdjustHP(-300, elementType, triggerDeath: true, null, null, showHPBar: true);
				}
			}
		}
		else
		{
			BurningSource burningSource = null;
			for (int k = 0; k < list.Count; k++)
			{
				ITraitable traitable2 = list[k];
				if (!(traitable2 is Character { faction: not null } character2) || !character2.faction.IsFriendlyWith(base.faction))
				{
					traitable2.AdjustHP(-300, elementType, triggerDeath: true, null, delegate(ITraitable target, Trait trait)
					{
						TraitManager.Instance.ProcessBurningTrait(target, trait, ref burningSource);
					}, showHPBar: true);
				}
			}
		}
		RuinarchListPool<ITraitable>.Release(list);
		GameManager.Instance.CreateElementalExplosionEffectAt(base.deathTilePosition.centeredWorldLocation, base.deathTilePosition.parentMap, elementType);
	}
}
