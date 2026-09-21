using UnityEngine;

namespace Interrupts;

public class HealOther : Interrupt
{
	public HealOther()
		: base(INTERRUPT.Heal_Other)
	{
		base.duration = 0;
		base.interruptIconString = GoapActionStateDB.Magic_Icon;
		base.isSimulateneous = true;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		_ = interruptHolder.actor;
		if (interruptHolder.target is Character { isDead: false } character)
		{
			GameManager.Instance.CreateParticleEffectAt(character, PARTICLE_EFFECT.Heal, allowRotation: false);
			int amount = Mathf.RoundToInt((float)character.maxHP * 0.25f);
			character.AdjustHP(amount, ELEMENTAL_TYPE.Normal);
		}
		return true;
	}
}
