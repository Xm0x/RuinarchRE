using System;
using UtilityScripts;

public class Mimic : Summon
{
	private Action<Mimic> _awakenMimicEvent;

	public override Type serializedData => typeof(SaveDataMimic);

	public bool isTreasureChest { get; private set; }

	public Mimic()
		: base(SUMMON_TYPE.Mimic, "Mimic", RACE.MIMIC, Utilities.GetRandomGender())
	{
	}

	public Mimic(string className)
		: base(SUMMON_TYPE.Mimic, className, RACE.MIMIC, Utilities.GetRandomGender())
	{
	}

	public Mimic(SaveDataMimic data)
		: base(data)
	{
		isTreasureChest = data.isTreasureChest;
	}

	protected override void OnTickEnded()
	{
		if (!isTreasureChest)
		{
			base.OnTickEnded();
		}
	}

	protected override void OnTickStarted()
	{
		if (!isTreasureChest)
		{
			base.OnTickStarted();
		}
	}

	public override void OnSeizePOI(bool wasUnseizedFromCharacter)
	{
		if (isTreasureChest)
		{
			ExecuteAwakenMimicEvent();
		}
		base.OnSeizePOI(wasUnseizedFromCharacter);
	}

	protected override void AfterAdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType)
	{
		base.AfterAdjustHP(amount, elementalDamageType);
		if (amount < 0 && !base.isDead && isTreasureChest)
		{
			ExecuteAwakenMimicEvent();
		}
	}

	public void MimicAgitatedHandling()
	{
		if (isTreasureChest)
		{
			ExecuteAwakenMimicEvent();
		}
	}

	public void SetIsTreasureChest(bool state)
	{
		isTreasureChest = state;
		if (isTreasureChest)
		{
			base.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
			base.traitContainer.AddTrait(this, "Hidden");
		}
		else
		{
			base.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			base.traitContainer.RemoveTrait(this, "Hidden");
		}
	}

	public void SubscribeToAwakenMimicEvent(TreasureChest p_chest)
	{
		_awakenMimicEvent = (Action<Mimic>)Delegate.Combine(_awakenMimicEvent, new Action<Mimic>(p_chest.TryAwakenMimic));
	}

	public void UnsubscribeToAwakenMimicEvent(TreasureChest p_chest)
	{
		_awakenMimicEvent = (Action<Mimic>)Delegate.Remove(_awakenMimicEvent, new Action<Mimic>(p_chest.TryAwakenMimic));
	}

	private void ExecuteAwakenMimicEvent()
	{
		_awakenMimicEvent?.Invoke(this);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_awakenMimicEvent = null;
	}
}
