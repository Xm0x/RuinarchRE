using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Interrupts;

public class WideHeal : Interrupt
{
	public WideHeal()
		: base(INTERRUPT.Wide_Heal)
	{
		base.duration = 0;
		base.interruptIconString = GoapActionStateDB.Magic_Icon;
		base.isSimulateneous = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		LocationGridTile gridTileLocation = actor.gridTileLocation;
		if (gridTileLocation != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			gridTileLocation.PopulateTilesInRadius(list, 8, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list.Count; i++)
			{
				LocationGridTile locationGridTile = list[i];
				for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
				{
					Character character = locationGridTile.charactersHere[j];
					if (character != actor && character.faction == actor.faction && !character.isDead && !character.IsHealthFull())
					{
						GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Heal, allowRotation: false);
						int amount = Mathf.RoundToInt((float)character.maxHP * 0.25f);
						character.AdjustHP(amount, ELEMENTAL_TYPE.Normal);
					}
				}
			}
		}
		return true;
	}
}
